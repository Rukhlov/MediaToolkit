using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DeckLinkAPI;


//using Microsoft.DirectX.Direct3D;
//using Direct3D = Microsoft.DirectX.Direct3D;

using SharpDX;
using SharpDX.Direct3D9;


namespace BlackMagic.DeckLink.UI
{


    public class ScreenPreviewCallback : IDeckLinkScreenPreviewCallback
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public ScreenPreviewCallback(IntPtr hWnd)
        {
            this.WindowHandle = hWnd;
        }

        private IDeckLinkDX9ScreenPreviewHelper previewHelper;

        private Direct3D direct3D = null;
        private Device device = null;
        private PresentParameters presentParameters;
        private IntPtr WindowHandle = IntPtr.Zero;

        public void Init()
        {
            logger.Debug("ScreenPreviewCallback::Init()");

            this.previewHelper = new CDeckLinkDX9ScreenPreviewHelper();


            direct3D = new Direct3D();
            presentParameters = new PresentParameters();
            presentParameters.BackBufferFormat = Format.Unknown;
            presentParameters.BackBufferCount = 2;
            presentParameters.Windowed = true;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.DeviceWindowHandle = WindowHandle;
            presentParameters.PresentationInterval = PresentInterval.Default;

            var flags = CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded;
            device = new Device(direct3D, 0, DeviceType.Hardware, WindowHandle, flags, presentParameters);

            previewHelper.Initialize(device.NativePointer);

        }

        void IDeckLinkScreenPreviewCallback.DrawFrame(IDeckLinkVideoFrame theFrame)
        {
            // First, pass the frame to the DeckLink screen preview helper
            //if (theFrame == null)
            //{
            //    return;
            //} 

            try
            {
                if (previewHelper != null)
                {
                    previewHelper.SetFrame(theFrame);

                    Render();
                }
            }
            finally
            {
                Marshal.ReleaseComObject(theFrame);
            }

        }


        void Render()
        {
            try
            {

                Result result = device.TestCooperativeLevel();
                if (result != ResultCode.Success)
                {
                    logger.Warn("TestCooperativeLevel: " + result);
                    bool deviceReady = false;
                    if (result == ResultCode.DeviceLost)
                    {
                        // OnLostDevice();
                        Thread.Sleep(50);
                    }
                    else if (result == ResultCode.DeviceNotReset)
                    {
                        DeviceReset();

                        //deviceReady = ReInitDevice();
                    }

                    if (!deviceReady)
                    {
                        //TODO: error
                        logger.Warn("Device is not ready...");

                        Thread.Sleep(100);

                        return;
                    }

                }

                device.BeginScene();

                // Render target needs to be set to 640x360 for optimal scaling. However the pixel coordinates for 
                // Direct3D 9 render target is actually (-0.5,-0.5) to (639.5,359.5).  As such the Viewport is set 
                // to 639x359 to account for the pixel coordinate offset of render target
                tagRECT rect;
                rect.top = device.Viewport.Y;
                rect.left = device.Viewport.X;
                rect.bottom = device.Viewport.Y + device.Viewport.Height;
                rect.right = device.Viewport.X + device.Viewport.Width;

                previewHelper.Render(rect);

                device.EndScene();

                device.Present();
            }
            catch (SharpDXException ex)
            {
                logger.Error(ex);

                //TODO:...
                // если произошла не понятная ошибка чтото делаем...
            }

        }

        private void DeviceReset()
        {
            logger.Debug("ScreenPreviewCallback::DeviceReset()");
            try
            {
                device.Reset(presentParameters);

                //Close();

                //// как закрыть хелпер непонятно, просто пересоздаем его
                //previewHelper = new CDeckLinkDX9ScreenPreviewHelper();// может утекать память !!!!

                //Init();
            }
            catch (Exception ex1)
            {
                logger.Error(ex1);
            }
        }


        public void Close()
        {
            logger.Debug("ScreenPreviewCallback::Close()");

            if (direct3D != null)
            {
                direct3D.Dispose();
                direct3D = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            if (previewHelper != null)
            {
                Marshal.ReleaseComObject(previewHelper);
                previewHelper = null;
            }

        }
    }


    public class PreviewWindow : Control, IDeckLinkScreenPreviewCallback
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDeckLinkDX9ScreenPreviewHelper previewHelper;
        private Device device;
        private PresentParameters presentParameters;

        public PreviewWindow()
        {
            previewHelper = new CDeckLinkDX9ScreenPreviewHelper();
            InitializeComponent();
        }

        public void InitD3D()
        {
            logger.Debug("InitD3D()");
            var d3d = new Direct3D();
            presentParameters = new PresentParameters();
            presentParameters.BackBufferFormat = Format.Unknown;
            presentParameters.BackBufferCount = 2;
            presentParameters.Windowed = true;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.DeviceWindowHandle = this.Handle;
            presentParameters.PresentationInterval = PresentInterval.Default;

            device = new Device(d3d, 0, DeviceType.Hardware, this.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded, presentParameters);
            var comObj = device as SharpDX.ComObject;

            previewHelper.Initialize(comObj.NativePointer);

        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        void Render()
        {
            try
            {

                Result result = device.TestCooperativeLevel();
                if (result != ResultCode.Success)
                {
                    logger.Warn("TestCooperativeLevel: " + result);
                    bool deviceReady = false;
                    if (result == ResultCode.DeviceLost)
                    {
                        // OnLostDevice();
                        Thread.Sleep(50);
                    }
                    else if (result == ResultCode.DeviceNotReset)
                    {
                        //deviceReady = ReInitDevice();
                    }

                    if (!deviceReady)
                    {
                        //TODO: error
                        logger.Warn("Device is not ready...");

                        Thread.Sleep(100);

                        return;
                    }

                }

                device.BeginScene();

                // Render target needs to be set to 640x360 for optimal scaling. However the pixel coordinates for 
                // Direct3D 9 render target is actually (-0.5,-0.5) to (639.5,359.5).  As such the Viewport is set 
                // to 639x359 to account for the pixel coordinate offset of render target
                tagRECT rect;
                rect.top = device.Viewport.Y;
                rect.left = device.Viewport.X;
                rect.bottom = device.Viewport.Y + device.Viewport.Height;
                rect.right = device.Viewport.X + device.Viewport.Width;

                previewHelper.Render(rect);

                // Draw the timecode top-center with a slight drop-shadow
                //Rectangle rc = m_d3DFont.MeasureString(null, m_timeCodeString, Direct3D.DrawTextFormat.Center, Color.Black);
                //int x = (m_d3DDevice.Viewport.Width / 2) - (rc.Width / 2);
                //int y = 10;
                //m_d3DFont.DrawText(null, m_timeCodeString, x + 1, y + 1, Color.Black);
                //m_d3DFont.DrawText(null, m_timeCodeString, x, y, Color.White);

                device.EndScene();

                device.Present();
            }
            catch (SharpDXException ex)
            {
                logger.Error(ex);

                //TODO:...
                // если произошла не понятная ошибка чтото делаем...
            }


        }

        private void DeviceReset()
        {
            logger.Debug("DeviceReset()");
            try
            {
                //if (previewHelper != null)
                //{
                //    Marshal.ReleaseComObject(previewHelper);
                //    previewHelper = null;
                //}

                //device.Reset(presentParameters);


                //if(previewHelper!= null)
                //{
                //    Marshal.ReleaseComObject(previewHelper);
                //    previewHelper = null;
                //}
                //// как закрыть хелпер непонятно, просто пересоздаем его
                //previewHelper = new CDeckLinkDX9ScreenPreviewHelper();// может утекать память !!!!

                CloseD3D();

                // как закрыть хелпер непонятно, просто пересоздаем его
                previewHelper = new CDeckLinkDX9ScreenPreviewHelper();// может утекать память !!!!

                InitD3D();
            }
            catch (Exception ex1)
            {
                logger.Error(ex1);
            }
        }

        //void SetTimecode(IDeckLinkVideoFrame videoFrame)
        //{
        //    IDeckLinkTimecode timecode;

        //    m_timeCodeString = "00:00:00:00";

        //    videoFrame.GetTimecode(_BMDTimecodeFormat.bmdTimecodeRP188Any, out timecode);

        //    if (timecode != null)
        //        timecode.GetString(out m_timeCodeString);
        //}

        void IDeckLinkScreenPreviewCallback.DrawFrame(IDeckLinkVideoFrame theFrame)
        {
            // First, pass the frame to the DeckLink screen preview helper
            previewHelper.SetFrame(theFrame);

            // SetTimecode(theFrame);

            // Then draw the frame to the scene
            Render();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(theFrame);
        }

        public void CloseD3D()
        {
            logger.Debug("CloseD3D()");

            //if (previewHelper != null)
            //{
            //    Marshal.ReleaseComObject(previewHelper);
            //    previewHelper = null;
            //}

            if (device != null)
            {
                device.Dispose();
                device = null;
            }



        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
