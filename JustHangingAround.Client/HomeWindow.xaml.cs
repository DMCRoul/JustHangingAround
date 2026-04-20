using System;
using System.Windows;
using JustHangingAround.Client.Services;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Client
{
    public partial class HomeWindow : Window
    {
        private readonly string _username;
        private readonly ApiClient _apiClient = new ApiClient();

        public HomeWindow(string username)
        {
            InitializeComponent();
            _username = username;
            WelcomeText.Text = $"Вы вошли как: {_username}";
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
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
                    MessagesList.Items.Add(result.ResponseText);
                    MessageInput.Clear();
                }
                else
                {
                    System.Windows.MessageBox.Show(result.ResponseText, "Ошибка");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Сбой подключения: {ex.Message}", "Ошибка");
            }
        }
    }
}