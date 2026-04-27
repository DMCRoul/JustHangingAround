using System;
using System.Windows;
using WinForms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace JustHangingAround.Client
{
    public partial class App : Application
    {
        private WinForms.NotifyIcon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIcon = new WinForms.NotifyIcon
            {
                Text = "JustHangingAround",
                Icon = Drawing.SystemIcons.Application,
                Visible = true
            };

            _trayIcon.DoubleClick += (_, _) =>
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Show();
                    Current.MainWindow.Activate();
                    Current.MainWindow.WindowState = WindowState.Normal;
                }
            };

            var menu = new WinForms.ContextMenuStrip();

            menu.Items.Add("Открыть", null, (_, _) =>
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Show();
                    Current.MainWindow.Activate();
                    Current.MainWindow.WindowState = WindowState.Normal;
                }
            });

            menu.Items.Add("Выход", null, (_, _) =>
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                Shutdown();
            });

            _trayIcon.ContextMenuStrip = menu;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }

            base.OnExit(e);
        }
    }
}