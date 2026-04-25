using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JustHangingAround.Client.Services;
using JustHangingAround.Client.ViewModels;
using JustHangingAround.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;

namespace JustHangingAround.Client
{
    public partial class HomeWindow : Window
    {
        private readonly string _username;
        private readonly string _token;
        private readonly ApiClient _apiClient = new ApiClient();
        private HubConnection _connection;
        private string? _selectedUser;
        public ObservableCollection<ChatMessageViewModel> Messages { get; } = new();

        public HomeWindow()
        {
            InitializeComponent();

            if (!UserSession.IsAuthenticated)
            {
                MessageBox.Show("Сессия не найдена. Войдите снова.", "Ошибка");
                Close();
                return;
            }

            _username = UserSession.Username!;
            _token = UserSession.Token!;

            _apiClient.SetToken(_token);

            WelcomeText.Text = $"Вы вошли как: {_username}";
            MessagesList.ItemsSource = Messages;

            Loaded += HomeWindow_Loaded;
        }

        private void HandleUnauthorized()
        {
            MessageBox.Show("Сессия истекла. Войдите снова.", "Авторизация");

            UserSession.Clear();

            var mainWindow = new MainWindow();
            mainWindow.Show();

            Application.Current.MainWindow = mainWindow;
            Close();
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_connection != null)
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                }

                UserSession.Clear();

                var mainWindow = new MainWindow();
                mainWindow.Show();

                Application.Current.MainWindow = mainWindow;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выхода: {ex.Message}", "Ошибка");
            }
        }

        private async void HomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsersAsync();
            await InitializeSignalR();
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedUser))
                {
                    Messages.Clear();
                    return;
                }

                var messages = await _apiClient.GetAsync<List<ChatMessage>>(
                    $"Chat/conversation/{_selectedUser}");

                if (_apiClient.LastRequestWasUnauthorized)
                {
                    HandleUnauthorized();
                    return;
                }

                Messages.Clear();

                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        Messages.Add(new ChatMessageViewModel
                        {
                            Username = message.Username,
                            Text = message.Text,
                            IsOwnMessage = message.Username == _username,
                            AttachmentUrl = message.AttachmentUrl != null
                                ? $"https://localhost:7137{message.AttachmentUrl}"
                                : null,
                            AttachmentFileName = message.AttachmentFileName,
                            AttachmentContentType = message.AttachmentContentType
                        });
                    }

                    ScrollMessagesToBottom();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сообщений: {ex.Message}", "Ошибка");
            }
        }

        private void ScrollMessagesToBottom()
        {
            if (MessagesList.Items.Count > 0)
            {
                var lastItem = MessagesList.Items[MessagesList.Items.Count - 1];
                MessagesList.ScrollIntoView(lastItem);
            }
        }

        private async Task InitializeSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7137/chatHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<ChatMessage>("ReceiveMessage", message =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(_selectedUser))
                    {
                        return;
                    }

                    var belongsToSelectedDialog =
                        (message.Username == _username && message.Recipient == _selectedUser) ||
                        (message.Username == _selectedUser && message.Recipient == _username);

                    if (!belongsToSelectedDialog)
                    {
                        return;
                    }

                    Messages.Add(new ChatMessageViewModel
                    {
                        Username = message.Username,
                        Text = message.Text,
                        IsOwnMessage = message.Username == _username,
                        AttachmentUrl = message.AttachmentUrl != null
                            ? $"https://localhost:7137{message.AttachmentUrl}"
                            : null,
                        AttachmentFileName = message.AttachmentFileName,
                        AttachmentContentType = message.AttachmentContentType
                    });

                    ScrollMessagesToBottom();
                });
            });

            await _connection.StartAsync();
        }

        private async Task SendCurrentMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessageBox.Show("Введите сообщение", "Ошибка");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedUser))
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            var text = MessageInput.Text;
            var recipient = _selectedUser;

            try
            {
                await _connection.InvokeAsync("SendMessage", recipient, text);
                MessageInput.Clear();
                MessageInput.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка");
            }
        }

        private string GetContentType(string filePath)
        {
            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        private async Task SendFileAsync(string filePath, string? text)
        {
            if (string.IsNullOrWhiteSpace(_selectedUser))
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                await using var fileStream = System.IO.File.OpenRead(filePath);

                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(GetContentType(filePath));

                using var form = new MultipartFormDataContent();

                form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));
                form.Add(new StringContent(_selectedUser), "recipient");

                if (!string.IsNullOrWhiteSpace(text))
                {
                    form.Add(new StringContent(text), "text");
                }

                var response = await client.PostAsync(
                    "https://localhost:7137/api/Chat/attachment",
                    form);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized();
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка отправки файла: {error}", "Ошибка");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                var message = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(
                    json,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (message == null)
                {
                    MessageBox.Show("Сервер не вернул сообщение", "Ошибка");
                    return;
                }

                await _connection.InvokeAsync("NotifyAttachmentSent", message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки файла: {ex.Message}", "Ошибка");
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendCurrentMessageAsync();
        }

        private async void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл для отправки"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            await SendFileAsync(dialog.FileName, MessageInput.Text);
            MessageInput.Clear();
            MessageInput.Focus();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            e.Handled = true;
            await SendCurrentMessageAsync();
        }

        private async void UsersList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is not string selectedUser)
            {
                return;
            }

            _selectedUser = selectedUser;
            await LoadMessagesAsync();
        }
        private void AttachmentImage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.Image image)
            {
                return;
            }

            if (image.DataContext is not ChatMessageViewModel message)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.AttachmentUrl))
            {
                return;
            }

            var window = new Window
            {
                Title = message.AttachmentFileName ?? "Изображение",
                Width = 800,
                Height = 600,
                Owner = this,
                Content = new System.Windows.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(message.AttachmentUrl)),
                    Stretch = System.Windows.Media.Stretch.Uniform
                }
            };

            window.ShowDialog();
        }
        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _apiClient.GetAsync<List<string>>("Users");

                if (_apiClient.LastRequestWasUnauthorized)
                {
                    HandleUnauthorized();
                    return;
                }

                UsersList.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
            }
        }
    }
}