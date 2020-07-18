
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

using ScreenStreamer.Wpf.Utils;

namespace ScreenStreamer.Wpf.Views
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

        private void DesignBorderWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           // Console.WriteLine(e.NewSize.Height + ";" + e.NewSize.Width+"; or:"+this.Height+";"+this.Width);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
               
        }

        
        private HwndSource hwndSource;

        private void Window1_SourceInitialized(object sender, EventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            //var dataContext = this.DataContext;
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
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.Top);
                    break;
                case "ResizeE":
                    this.Cursor = Cursors.SizeWE;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.Right);
                    break;
                case "ResizeS":
                    this.Cursor = Cursors.SizeNS;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.Bottom);
                    break;
                case "ResizeW":
                    this.Cursor = Cursors.SizeWE;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.Left);
                    break;
                case "ResizeNW":
                    this.Cursor = Cursors.SizeNWSE;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.TopLeft);
                    break;
                case "ResizeNE":
                    this.Cursor = Cursors.SizeNESW;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.TopRight);
                    break;
                case "ResizeSE":
                    this.Cursor = Cursors.SizeNWSE;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.BottomRight);
                    break;
                case "ResizeSW":
                    this.Cursor = Cursors.SizeNESW;
                    NativeMethods.ResizeWindow(hwndSource.Handle, ResizeDirection.BottomLeft);
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
