using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace JustHangingAround.Client
{
    public partial class FilePickerWindow : Window
    {
        private string _currentFolder;

        public string? SelectedFile { get; private set; }

        public FilePickerWindow()
        {
            InitializeComponent();

            _currentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            ProtectWindow(this);
            LoadFolder(_currentFolder);
        }

        private void LoadFolder(string folder)
        {
            try
            {
                _currentFolder = folder;
                CurrentPathText.Text = folder;

                var items = new List<FilePickerItem>();

                var directories = Directory.GetDirectories(folder)
                    .Select(path => new FilePickerItem
                    {
                        Name = Path.GetFileName(path),
                        FullPath = path,
                        IsDirectory = true,
                        Icon = "📁"
                    });

                var files = Directory.GetFiles(folder)
                    .Select(path => new FilePickerItem
                    {
                        Name = Path.GetFileName(path),
                        FullPath = path,
                        IsDirectory = false,
                        Icon = GetFileIcon(path)
                    });

                items.AddRange(directories.OrderBy(x => x.Name));
                items.AddRange(files.OrderBy(x => x.Name));

                ItemsList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть папку: {ex.Message}", "Ошибка");
            }
        }

        private static string GetFileIcon(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" => "🖼️",
                ".pdf" => "📄",
                ".doc" or ".docx" => "📝",
                ".xls" or ".xlsx" => "📊",
                ".zip" or ".rar" or ".7z" => "📦",
                ".mp4" or ".mov" or ".avi" => "🎬",
                ".mp3" or ".wav" => "🎵",
                _ => "📎"
            };
        }

        private void ItemsList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ItemsList.SelectedItem is not FilePickerItem item)
            {
                return;
            }

            if (item.IsDirectory)
            {
                LoadFolder(item.FullPath);
                return;
            }

            SelectedFile = item.FullPath;
            DialogResult = true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var parent = Directory.GetParent(_currentFolder);

            if (parent == null)
            {
                return;
            }

            LoadFolder(parent.FullName);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        private const uint WDA_EXCLUDEFROMCAPTURE = 0x11;

        private void ProtectWindow(Window window)
        {
            window.Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(window).Handle;

                if (hwnd != IntPtr.Zero)
                {
                    SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE);
                }
            };
        }

        private class FilePickerItem
        {
            public string Name { get; set; } = "";
            public string FullPath { get; set; } = "";
            public bool IsDirectory { get; set; }
            public string Icon { get; set; } = "📎";
        }
    }
}