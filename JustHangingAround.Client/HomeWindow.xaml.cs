using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using JustHangingAround.Client.Services;
using JustHangingAround.Client.ViewModels;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Client
{
    public partial class HomeWindow : Window
    {
        private readonly string _username;
        private readonly ApiClient _apiClient = new ApiClient();

        public ObservableCollection<ChatMessageViewModel> Messages { get; } = new();

        public HomeWindow(string username)
        {
            InitializeComponent();

            _username = username;
            WelcomeText.Text = $"Вы вошли как: {_username}";
            MessagesList.ItemsSource = Messages;

            Loaded += HomeWindow_Loaded;
        }

        private async void HomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                var messages = await _apiClient.GetAsync<List<ChatMessage>>("Chat/history");

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

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                MessageBox.Show("Введите сообщение", "Ошибка");
                return;
            }

            var request = new SendMessageRequest
            {
                Username = _username,
                Text = MessageInput.Text
            };

            try
            {
                var result = await _apiClient.PostAsync("Chat/send", request);

                if (result.IsSuccess)
                {
                    MessageInput.Clear();
                    await LoadMessagesAsync();
                }
                else
                {
                    MessageBox.Show(result.ResponseText, "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сбой подключения: {ex.Message}", "Ошибка");
            }
        }
    }


}