﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
    public static class Gdi32
    {

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
                                        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
                                        TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(IntPtr hdc, StretchingMode iStretchMode);

		[DllImport("gdi32.dll", ExactSpelling = true)]
		public static extern int SetDIBitsToDevice(IntPtr hdc, int xdst, int ydst,
			int width, int height, int xsrc, int ysrc, int start, int lines,
			IntPtr bitsptr, IntPtr bmiptr, int color);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint pila, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);


        [DllImport("gdi32.dll")]
		public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


        public enum DeviceCap
		{
			DRIVERVERSION = 0,
			TECHNOLOGY = 2,
			HORZSIZE = 4,
			VERTSIZE = 6,
			HORZRES = 8,
			VERTRES = 10,
			BITSPIXEL = 12,
			PLANES = 14,
			NUMBRUSHES = 16,
			NUMPENS = 18,
			NUMMARKERS = 20,
			NUMFONTS = 22,
			NUMCOLORS = 24,
			PDEVICESIZE = 26,
			CURVECAPS = 28,
			LINECAPS = 30,
			POLYGONALCAPS = 32,
			TEXTCAPS = 34,
			CLIPCAPS = 36,
			RASTERCAPS = 38, //Bitblt capabilities
			ASPECTX = 40,
			ASPECTY = 42,
			ASPECTXY = 44,
			SHADEBLENDCAPS = 45, //Shading and Blending caps
			LOGPIXELSX = 88,
			LOGPIXELSY = 90,
			SIZEPALETTE = 104,
			NUMRESERVED = 106,
			COLORRES = 108, //Actual color resolution
			PHYSICALWIDTH = 110,
			PHYSICALHEIGHT = 111,
			PHYSICALOFFSETX = 112,
			PHYSICALOFFSETY = 113,
			SCALINGFACTORX = 114,
			SCALINGFACTORY = 115,
			VREFRESH = 116,
			DESKTOPVERTRES = 117,
			DESKTOPHORZRES = 118,
			BLTALIGNMENT = 119 // Preferred blt alignment
		}

        [DllImport("gdi32.dll")]
        public static extern IntPtr GetStockObject(StockObjects fnObject);

        public enum StockObjects
        {
            WHITE_BRUSH = 0,
            LTGRAY_BRUSH = 1,
            GRAY_BRUSH = 2,
            DKGRAY_BRUSH = 3,
            BLACK_BRUSH = 4,
            NULL_BRUSH = 5,
            HOLLOW_BRUSH = NULL_BRUSH,
            WHITE_PEN = 6,
            BLACK_PEN = 7,
            NULL_PEN = 8,
            OEM_FIXED_FONT = 10,
            ANSI_FIXED_FONT = 11,
            ANSI_VAR_FONT = 12,
            SYSTEM_FONT = 13,
            DEVICE_DEFAULT_FONT = 14,
            DEFAULT_PALETTE = 15,
            SYSTEM_FIXED_FONT = 16,
            DEFAULT_GUI_FONT = 17,
            DC_BRUSH = 18,
            DC_PEN = 19,
        }
    }

}
