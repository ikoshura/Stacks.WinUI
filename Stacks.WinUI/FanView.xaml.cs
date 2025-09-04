using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT.Interop;
// *** PERBAIKAN: Menambahkan using static untuk PInvoke ***
using static PInvoke.User32;

namespace Stacks
{
    public sealed partial class FanView : Window
    {
        public ObservableCollection<FileItem> Files { get; set; }
        private AppWindow _appWindow;

        public FanView()
        {
            this.InitializeComponent();
            Files = new ObservableCollection<FileItem>();
            FileItemsControl.ItemsSource = Files;

            SetupWindow();
        }

        private void SetupWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            _appWindow.IsShownInSwitchers = false;

            var presenter = _appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }

            this.Activated += (s, e) =>
            {
                if (e.WindowActivationState == WindowActivationState.Deactivated)
                {
                    this.Hide();
                }
            };
        }

        public async void ShowAt(PointInt32 position)
        {
            await LoadFiles();
            this.Content.UpdateLayout();
            _appWindow.ResizeClient(new SizeInt32(400, 300));
            _appWindow.Move(position);
            this.Activate();
        }

        public void Hide()
        {
            _appWindow.Hide();
        }

        private async Task LoadFiles()
        {
            try
            {
                string sourcePath = SettingsManager.Current.SourceFolderPath;
                if (!Directory.Exists(sourcePath)) { Files.Clear(); return; }

                var filePaths = await Task.Run(() => Directory.GetFiles(sourcePath)
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime).Take(10).ToList());

                Files.Clear();
                foreach (var path in filePaths)
                {
                    Files.Add(new FileItem { FilePath = path, FileName = Path.GetFileName(path), Thumbnail = new BitmapImage(new Uri(path)) });
                }
            }
            catch (Exception)
            {
                // Tampilkan dialog error
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private async void FileItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is FileItem fileItem)
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(fileItem.FilePath);
                await Windows.System.Launcher.LaunchFileAsync(file);
                this.Hide();
            }
        }
    }
}