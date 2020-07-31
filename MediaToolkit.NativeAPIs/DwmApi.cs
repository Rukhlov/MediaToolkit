using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
    public sealed class DwmApi
    {
        public static void DisableAero(bool disable)
        {
            var compositionAction = disable ? CompositionAction.DWM_EC_DISABLECOMPOSITION : CompositionAction.DWM_EC_ENABLECOMPOSITION;

            DwmEnableComposition(compositionAction);

        }



        public static bool GetCloaked(IntPtr hWnd)
        {
            bool cloaked = false;
            var result = DwmGetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_CLOAKED, out cloaked);

            return cloaked;
        }

        public static Rectangle GetExtendedFrameBounds(IntPtr hWnd)
        {
            Rectangle frameBounds = Rectangle.Empty;

            var result = DwmGetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rect);
            if(result == 0)
            {
                frameBounds = rect.AsRectangle;
            }

            return frameBounds;
        }

        public static bool IsCompositionEnabled()
        {
            bool enabled = true;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                var hResult = DwmIsCompositionEnabled(out enabled);
            }

            return enabled;
        }

        [DllImport("dwmapi.dll")]
        internal static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableComposition(CompositionAction uCompositionAction);


        [DllImport("dwmapi.dll")]
        internal static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, IntPtr pvAttribute, int cbAttribute);

        public static int DwmGetWindowAttribute<T>(IntPtr hwnd, DwmWindowAttribute dwAttribute, out T pvAttribute)
        {
            int result = 0;

            pvAttribute = default(T);

            int size = Marshal.SizeOf(pvAttribute);

            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(pvAttribute, ptr, false);
                 
                result = DwmGetWindowAttribute(hwnd, dwAttribute, ptr, size);

                pvAttribute = (T)Marshal.PtrToStructure(ptr, pvAttribute.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return result;
        }

        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }
    }
}
