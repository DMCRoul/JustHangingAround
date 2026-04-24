using System;
using System.Windows;
using JustHangingAround.Client.Services;
using JustHangingAround.Shared.Models;

namespace JustHangingAround.Client
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient = new ApiClient();

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

            try
            {
                var result = await _apiClient.PostAsync("Auth/register", request);

                if (result.IsSuccess)
                {
                    StatusText.Text = $"Успех: {result.ResponseText}";
                }
                else
                {
                    StatusText.Text = $"Ошибка: {result.ResponseText}";
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

            try
            {
                var loginResponse = await _apiClient.LoginAsync(request);

                if (loginResponse is not null && !string.IsNullOrWhiteSpace(loginResponse.Token))
                {
                    UserSession.Set(loginResponse.Username, loginResponse.Token);

                    var homeWindow = new HomeWindow();
                    homeWindow.Show();

                    Application.Current.MainWindow = homeWindow;
                    Hide();
                }
                else
                {
                    StatusText.Text = "Ошибка: неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Сбой подключения: {ex.Message}";
            }
        }
    }
}