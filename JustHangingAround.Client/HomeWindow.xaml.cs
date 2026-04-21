using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using JustHangingAround.Client.Services;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Client
{
    public partial class HomeWindow : Window
    {
        private readonly string _username;
        private readonly ApiClient _apiClient = new ApiClient();
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7137/api/")
        };

        public HomeWindow(string username)
        {
            InitializeComponent();
            _username = username;
            WelcomeText.Text = $"Вы вошли как: {_username}";
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
                var messages = await _httpClient.GetFromJsonAsync<List<ChatMessage>>("Chat/history");

                MessagesList.Items.Clear();

                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        MessagesList.Items.Add($"{message.Username}: {message.Text}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сообщений: {ex.Message}", "Ошибка");
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