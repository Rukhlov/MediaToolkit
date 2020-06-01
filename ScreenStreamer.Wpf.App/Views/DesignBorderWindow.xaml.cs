using ScreenStreamer.Wpf.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenStreamer.Wpf.Common.Views
{
    /// <summary>
    /// Interaction logic for DesignBorderWindow.xaml
    /// </summary>
    public partial class DesignBorderWindow : Window
    {
        public DesignBorderWindow()
        {
            SourceInitialized += Window1_SourceInitialized;
            
            InitializeComponent();
            this.SizeChanged += DesignBorderWindow_SizeChanged;
        }

        //TODO: пересчитать размеры формы для разных DPI...

        private void DesignBorderWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine(e.NewSize.Height + ";" + e.NewSize.Width+"; or:"+this.Height+";"+this.Width);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;

        private enum ResizeDirection
        {
            Left = 61441,
            Right = 61442,
            Top = 61443,
            TopLeft = 61444,
            TopRight = 61445,
            Bottom = 61446,
            BottomLeft = 61447,
            BottomRight = 61448,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        

        private void Window1_SourceInitialized(object sender, EventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)direction, IntPtr.Zero);
        }

        protected void ResetCursor(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        protected void Resize(object sender, MouseButtonEventArgs e)
        {
            var clickedShape = sender as Border;

            switch (clickedShape.Name)
            {
                case "ResizeN":
                    this.Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "ResizeE":
                    this.Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "ResizeS":
                    this.Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "ResizeW":
                    this.Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "ResizeNW":
                    this.Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "ResizeNE":
                    this.Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "ResizeSE":
                    this.Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                case "ResizeSW":
                    this.Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                default:
                    break;
            }
        }

        protected void DisplayResizeCursor(object sender, MouseEventArgs e)
        {
            var clickedShape = sender as Border;

            switch (clickedShape.Name)
            {
                case "ResizeN":
                case "ResizeS":
                    this.Cursor = Cursors.SizeNS;
                    break;
                case "ResizeE":
                case "ResizeW":
                    this.Cursor = Cursors.SizeWE;
                    break;
                case "ResizeNW":
                case "ResizeSE":
                    this.Cursor = Cursors.SizeNWSE;
                    break;
                case "ResizeNE":
                case "ResizeSW":
                    this.Cursor = Cursors.SizeNESW;
                    break;
                default:
                    break;
            }
        }

        //protected void DragWindow(object sender, MouseButtonEventArgs e)
        //{
        //    DragMove();
        //}
    }
}
