using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;
using PInvoke; // *** PERBAIKAN: Namespace PInvoke harusnya dikenali sekarang ***

namespace Stacks
{
    public partial class App : Application
    {
        private TaskBarWidget _taskBarWidget;
        private FanView _fanView;
        private Window m_window; // Jendela utama yang tersembunyi untuk menjaga aplikasi tetap hidup

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Buat jendela utama yang tidak akan pernah kita tampilkan.
            // Ini diperlukan agar aplikasi tidak langsung tertutup.
            m_window = new Window();

            // Buat FanView tetapi jangan tampilkan
            _fanView = new FanView();
            _fanView.Hide(); // Gunakan metode Hide() kustom kita

            // Buat dan inisialisasi widget taskbar
            _taskBarWidget = new TaskBarWidget();
            _taskBarWidget.WidgetClicked += OnWidgetClicked;
            _taskBarWidget.Initialize();
        }

        private void OnWidgetClicked()
        {
            if (_fanView == null) return;

            // Dapatkan posisi kursor global
            if (User32.GetCursorPos(out POINT point))
            {
                _fanView.ShowAt(new PointInt32(point.x, point.y));
            }
        }
    }
}