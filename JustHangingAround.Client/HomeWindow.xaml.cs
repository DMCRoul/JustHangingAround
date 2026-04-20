using System.Windows;

namespace JustHangingAround.Client
{
    public partial class HomeWindow : Window
    {
        public HomeWindow(string username)
        {
            InitializeComponent();
            WelcomeText.Text = $"Вы вошли как: {username}";
        }
    }
}