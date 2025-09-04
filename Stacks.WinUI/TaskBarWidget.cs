using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Windows;
using Windows.Graphics;
using WinRT.Interop;
using static PInvoke.Kernel32;
// *** PERBAIKAN: Menambahkan using static untuk PInvoke ***
using static PInvoke.User32;

namespace Stacks
{
    internal class TaskBarWidget : IDisposable
    {
        private const string TaskBarClassName = "Shell_TrayWnd";
        private const string TrayNotifyWndClassName = "TrayNotifyWnd";

        private readonly IntPtr _hwndShell;
        private readonly IntPtr _hwndTrayNotify;
        private readonly string _widgetClassName = "StacksWidgetWinUI";
        private readonly double _dpiScale;

        private IntPtr _hwnd;
        private AppWindow _appWindow;
        private DesktopWindowXamlSource _xamlSource;
        private MainWindow _widgetControl;
        private WNDPROC _wndProc;

        public event Action WidgetClicked
        {
            add => _widgetControl.WidgetClicked += value;
            remove => _widgetControl.WidgetClicked -= value;
        }

        public TaskBarWidget()
        {
            _hwndShell = FindWindow(TaskBarClassName, null);
            _hwndTrayNotify = FindWindowEx(_hwndShell, IntPtr.Zero, TrayNotifyWndClassName, null);

            var dpi = GetDpiForWindow(_hwndShell);
            _dpiScale = dpi / 96d;
        }

        public void Initialize()
        {
            _hwnd = CreateHostWindow();

            var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);
            _appWindow.IsShownInSwitchers = false;

            _xamlSource = new DesktopWindowXamlSource();
            _xamlSource.Initialize(windowId);

            _widgetControl = new MainWindow();
            _xamlSource.Content = _widgetControl;

            InjectAndPosition();
            _appWindow.Show();
        }

        private IntPtr CreateHostWindow()
        {
            _wndProc = WindowProc;
            var wc = new WNDCLASSEX
            {
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<WNDCLASSEX>(),
                hInstance = GetModuleHandle(null),
                lpfnWndProc = _wndProc,
                lpszClassName = _widgetClassName,
            };
            RegisterClassEx(ref wc);

            return CreateWindowEx(
                WindowStylesEx.WS_EX_LAYERED | WindowStylesEx.WS_EX_TOOLWINDOW,
                _widgetClassName, "StacksWidgetHost", WindowStyles.WS_POPUP,
                0, 0, 0, 0, _hwndShell, IntPtr.Zero, wc.hInstance, IntPtr.Zero
            );
        }

        private void InjectAndPosition()
        {
            if (SetParent(_hwnd, _hwndShell) == IntPtr.Zero)
            {
                return;
            }

            GetWindowRect(_hwndTrayNotify, out RECT trayRect);
            GetWindowRect(_hwndShell, out RECT taskbarRect);

            int widgetWidth = (int)(_widgetControl.Width * _dpiScale);
            int widgetHeight = (int)(_widgetControl.Height * _dpiScale);

            int newX = trayRect.left - widgetWidth - 4;
            int newY = taskbarRect.top + ((taskbarRect.bottom - taskbarRect.top - widgetHeight) / 2);

            _appWindow.MoveAndResize(new RectInt32(newX, newY, widgetWidth, widgetHeight));
        }

        private IntPtr WindowProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            return DefWindowProc(hWnd, Msg, wParam, lParam);
        }

        public void Dispose()
        {
            _appWindow?.Destroy();
            _xamlSource?.Dispose();
            UnregisterClass(_widgetClassName, GetModuleHandle(null));
        }
    }
}