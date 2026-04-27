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
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;
using Imaging = System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;

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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        private const uint WDA_NONE = 0x0;
        private const uint WDA_EXCLUDEFROMCAPTURE = 0x11;

        public HomeWindow()
        {
            InitializeComponent();


            ProtectWindow(this);

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

            UpdateCurrentDialogHeader();

            Loaded += HomeWindow_Loaded;
        }

        private void UpdateCurrentDialogHeader()
        {
            if (string.IsNullOrWhiteSpace(_selectedUser))
            {
                CurrentDialogText.Text = "Выберите пользователя слева и начните переписку";
                return;
            }

            CurrentDialogText.Text = $"Диалог с: {_selectedUser}";
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
                            CreatedAt = message.CreatedAt,
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
            MessagesList.Dispatcher.InvokeAsync(() =>
            {
                var scrollViewer = FindChild<ScrollViewer>(MessagesList);
                scrollViewer?.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
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
                        return;

                    var belongs =
                        (message.Username == _username && message.Recipient == _selectedUser) ||
                        (message.Username == _selectedUser && message.Recipient == _username);

                    if (!belongs) return;

                    Messages.Add(new ChatMessageViewModel
                    {
                        Username = message.Username,
                        Text = message.Text,
                        IsOwnMessage = message.Username == _username,
                        AttachmentUrl = message.AttachmentUrl != null
                            ? $"https://localhost:7137{message.AttachmentUrl}"
                            : null,
                        CreatedAt = message.CreatedAt,
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
                MessageBox.Show("Введите сообщение");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedUser))
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }

            await _connection.InvokeAsync("SendMessage", _selectedUser, MessageInput.Text);

            MessageInput.Clear();
            MessageInput.Focus();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendCurrentMessageAsync();
        }

        private async void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is not string selectedUser)
                return;

            _selectedUser = selectedUser;
            UpdateCurrentDialogHeader();

            await LoadMessagesAsync();
        }


        private async void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FilePickerWindow
            {
                Owner = this
            };

            if (picker.ShowDialog() != true)
            {
                return;
            }

            await SendFileAsync(picker.SelectedFile!, MessageInput.Text);
            MessageInput.Clear();
        }

        private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await SendCurrentMessageAsync();
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S)
            {
                e.Handled = true;
                await SendScreenshotAsync();
            }
        }

        private async void SendScreenshot_Click(object sender, RoutedEventArgs e)
        {
            await SendScreenshotAsync();
        }

        private async Task SendFileAsync(string filePath, string? text)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            await using var stream = File.OpenRead(filePath);
            using var content = new StreamContent(stream);

            using var form = new MultipartFormDataContent();
            form.Add(content, "file", Path.GetFileName(filePath));
            form.Add(new StringContent(_selectedUser!), "recipient");

            if (!string.IsNullOrWhiteSpace(text))
                form.Add(new StringContent(text), "text");

            var response = await client.PostAsync("https://localhost:7137/api/Chat/attachment", form);

            var json = await response.Content.ReadAsStringAsync();

            var message = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await _connection.InvokeAsync("NotifyAttachmentSent", message);
        }

        private string CaptureScreenshot()
        {
            var bounds = WinForms.Screen.PrimaryScreen!.Bounds;

            using var bitmap = new Drawing.Bitmap(bounds.Width, bounds.Height);
            using var graphics = Drawing.Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);

            var path = Path.Combine(Path.GetTempPath(), $"screenshot_{Guid.NewGuid()}.png");
            bitmap.Save(path, Imaging.ImageFormat.Png);

            return path;
        }

        private async Task SendScreenshotAsync()
        {
            var path = CaptureScreenshot();
            await SendFileAsync(path, "[Скриншот]");
        }

        private void ProtectWindow(Window window)
        {
            window.Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(window).Handle;

                if (hwnd != IntPtr.Zero)
                {
                    SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
                }
            };
        }

        private void AttachmentImage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.Image img) return;
            if (img.DataContext is not ChatMessageViewModel msg) return;

            var w = new Window
            {
                Width = 800,
                Height = 600,
                Content = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(msg.AttachmentUrl)),
                    Stretch = Stretch.Uniform
                }
            };

            ProtectWindow(w);

            w.ShowDialog();
        }

        private void AttachmentFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement el) return;
            if (el.DataContext is not ChatMessageViewModel msg) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = msg.AttachmentUrl,
                UseShellExecute = true
            });
        }
    }
}