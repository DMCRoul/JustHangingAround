using System;
using System.Windows;
using JustHangingAround.Client.Services;
using JustHangingAround.Shared.Models;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace JustHangingAround.Client
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient = new ApiClient();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        private const uint WDA_EXCLUDEFROMCAPTURE = 0x11;
        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                e.Cancel = true;
                Hide();
            };

            Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;

                if (hwnd != IntPtr.Zero)
                {
                    SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
                }
            };
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

    MessageBox.Show($"Success: {result.IsSuccess}\n{result.ResponseText}");

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
    MessageBox.Show(ex.ToString(), "FULL ERROR");
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