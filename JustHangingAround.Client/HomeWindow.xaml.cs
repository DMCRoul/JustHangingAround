using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using JustHangingAround.Client.Services;
using JustHangingAround.Client.ViewModels;
using JustHangingAround.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;

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
                            IsOwnMessage = message.Username == _username
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
                        IsOwnMessage = message.Username == _username
                    });

                    ScrollMessagesToBottom();
                });
            });

            await _connection.StartAsync();
        }
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessageBox.Show("Введите сообщение", "Ошибка");
                return;
            }

            var text = MessageInput.Text;

            if (string.IsNullOrWhiteSpace(_selectedUser))
            {
                MessageBox.Show("Выберите пользователя", "Ошибка");
                return;
            }

            var recipient = _selectedUser;
            try
            {
                await _connection.InvokeAsync("SendMessage", recipient, text);
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка");
            }
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