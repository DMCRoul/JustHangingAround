using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Client
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            var request = new RegisterRequest
            {
                Username = UsernameBox.Text,
                Password = PasswordBox.Password
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://localhost:7137/api/Auth/register", content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    StatusText.Text = $"Успех: {responseText}";
                }
                else
                {
                    StatusText.Text = $"Ошибка: {responseText}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Сбой подключения: {ex.Message}";
            }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var request = new LoginRequest
            {
                Username = UsernameBox.Text,
                Password = PasswordBox.Password
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://localhost:7137/api/Auth/login", content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var homeWindow = new HomeWindow(request.Username);
                    homeWindow.Show();
                    Close();
                }
                else
                {
                    StatusText.Text = $"Ошибка: {responseText}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Сбой подключения: {ex.Message}";
            }
        }
    }
}