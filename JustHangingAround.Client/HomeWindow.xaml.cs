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
            await LoadMessagesAsync();
            await InitializeSignalR();
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                var messages = await _apiClient.GetAsync<List<ChatMessage>>("Chat/history");

                if (_apiClient.LastRequestWasUnauthorized)
                {
                    MessageBox.Show("Сессия истекла. Войдите снова.", "Авторизация");

                    UserSession.Clear();

                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    Application.Current.MainWindow = mainWindow;
                    Close();

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

            try
            {
                await _connection.InvokeAsync("SendMessage", text);
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка");
            }
        }
    }
}