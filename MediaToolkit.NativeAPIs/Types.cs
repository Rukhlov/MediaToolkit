using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace MediaToolkit.NativeAPIs
{

    public enum WinErrors : long
    {
        ERROR_SUCCESS = 0,

        ERROR_INVALID_FUNCTION = 1,
        ERROR_FILE_NOT_FOUND = 2,
        ERROR_PATH_NOT_FOUND = 3,
        ERROR_TOO_MANY_OPEN_FILES = 4,
        ERROR_ACCESS_DENIED = 5,
        ERROR_INVALID_HANDLE = 6,
        ERROR_ARENA_TRASHED = 7,
        ERROR_NOT_ENOUGH_MEMORY = 8,
        ERROR_INVALID_BLOCK = 9,
        ERROR_BAD_ENVIRONMENT = 10,
        ERROR_BAD_FORMAT = 11,
        ERROR_INVALID_ACCESS = 12,
        ERROR_INVALID_DATA = 13,
        ERROR_OUTOFMEMORY = 14,

        ERROR_NOT_READY = 21,

        ERROR_NOT_SUPPORTED = 50,

        ERROR_INSUFFICIENT_BUFFER = 122,

        ERROR_MORE_DATA = 234,
        ERROR_NO_MORE_ITEMS = 259,
        ERROR_SERVICE_SPECIFIC_ERROR = 1066,
        ERROR_INVALID_USER_BUFFER = 1784


        //...
    }



    public enum TernaryRasterOperations : uint
    {
        SRCCOPY = 0x00CC0020,
        SRCPAINT = 0x00EE0086,
        SRCAND = 0x008800C6,
        SRCINVERT = 0x00660046,
        SRCERASE = 0x00440328,
        NOTSRCCOPY = 0x00330008,
        NOTSRCERASE = 0x001100A6,
        MERGECOPY = 0x00C000CA,
        MERGEPAINT = 0x00BB0226,
        PATCOPY = 0x00F00021,
        PATPAINT = 0x00FB0A09,
        PATINVERT = 0x005A0049,
        DSTINVERT = 0x00550009,
        BLACKNESS = 0x00000042,
        WHITENESS = 0x00FF0062,
        CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }


    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public RGBQUAD[] bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        /// <summary>
        /// Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies an icon; FALSE specifies a cursor.
        /// </summary>
        public bool fIcon;

        /// <summary>
        /// The x-coordinate of a cursor's hot spot. If this structure defines an icon, 
        /// the hot spot is always in the center of the icon, and this member is ignored.
        /// </summary>
        public int xHotspot;

        /// <summary>
        /// The y-coordinate of the cursor's hot spot. If this structure defines an icon, 
        /// the hot spot is always in the center of the icon, and this member is ignored.
        /// </summary>
        public Int32 yHotspot;

        /// <summary>
        /// The icon bitmask bitmap. If this structure defines a black and white icon, 
        /// this bitmask is formatted so that the upper half is the icon AND bitmask and the lower half is the icon XOR bitmask.
        /// Under this condition, the height should be an even multiple of two.
        /// If this structure defines a color icon, this mask only defines the AND bitmask of the icon.
        /// </summary>
        public IntPtr hbmMask;

        /// <summary>
        /// A handle to the icon color bitmap. 
        /// This member can be optional if this structure defines a black and white icon. 
        /// The AND bitmask of hbmMask is applied with the SRCAND flag to the destination; 
        /// subsequently, the color bitmap is applied (using XOR) to the destination by using the SRCINVERT flag.
        /// </summary>
        public IntPtr hbmColor;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public Rectangle AsRectangle
        {
            get
            {
                return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            }
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public static RECT FromRectangle(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public struct RECT
    //{
    //    public int Left;
    //    public int Top;
    //    public int Right;
    //    public int Bottom;
    //}



    [Flags]
    public enum StretchingMode
    {
        /// <summary>
        /// Performs a Boolean AND operation using the color values for the eliminated 
        /// and existing pixels. If the bitmap is a monochrome bitmap, this mode preserves
        /// black pixels at the expense of white pixels.
        /// </summary>
        BLACKONWHITE = 1,
        /// <summary>
        /// Deletes the pixels. This mode deletes all eliminated lines of pixels 
        /// without trying to preserve their information
        /// </summary>
        COLORONCOLOR = 3,
        /// <summary>
        /// Maps pixels from the source rectangle into blocks of pixels in the destination rectangle.
        /// The average color over the destination block of pixels approximates the color of the source pixels. 
        /// This option is not supported on Windows 95/98/Me
        /// </summary>
        HALFTONE = 4,
        /// <summary>
        /// Performs a Boolean AND operation using the color values for the eliminated 
        /// and existing pixels. If the bitmap is a monochrome bitmap, this mode preserves
        /// black pixels at the expense of white pixels (same as BLACKONWHITE)
        /// </summary>
        STRETCH_ANDSCANS = 1,
        /// <summary>
        /// Deletes the pixels. This mode deletes all eliminated lines of pixels 
        /// without trying to preserve their information (same as COLORONCOLOR)
        /// </summary>
        STRETCH_DELETESCANS = 3,
        /// <summary>
        /// Maps pixels from the source rectangle into blocks of pixels in the destination rectangle.
        /// The average color over the destination block of pixels approximates the color of the source pixels. 
        /// This option is not supported on Windows 95/98/Me (same as HALFTONE)
        /// </summary>
        STRETCH_HALFTONE = 4,
        /// <summary>
        /// Performs a Boolean OR operation using the color values for the eliminated and existing pixels.
        /// If the bitmap is a monochrome bitmap, this mode preserves white pixels at the expense of 
        /// black pixels(same as WHITEONBLACK)
        /// </summary>
        STRETCH_ORSCANS = 2,
        /// <summary>
        /// Performs a Boolean OR operation using the color values for the eliminated and existing pixels.
        /// If the bitmap is a monochrome bitmap, this mode preserves white pixels at the expense of black pixels.
        /// </summary>
        WHITEONBLACK = 2,
        /// <summary>
        /// Fail to stretch
        /// </summary>
        ERROR = 0
    }


    [Flags]
    public enum CompositionAction : uint
    {
        DWM_EC_DISABLECOMPOSITION = 0,
        DWM_EC_ENABLECOMPOSITION = 1
    }


    public enum ProcessDPIAwareness
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }


    // https://docs.microsoft.com/en-us/windows/desktop/devio/device-management-functions
    public class DBT
    {
        public const uint CONFIGCHANGECANCELED = 0x0019;
        //A request to change the current configuration (dock or undock) has been canceled.

        public const uint CONFIGCHANGED = 0x0018;
        //The current configuration has changed, due to a dock or undock.

        public const uint CUSTOMEVENT = 0x8006;
        //A custom event has occurred.

        public const uint DEVICEARRIVAL = 0x8000;
        //A device or piece of media has been inserted and is now available.

        public const uint DEVICEQUERYREMOVE = 0x8001;
        //Permission is requested to remove a device or piece of media. Any application can deny this request and cancel the removal.

        public const uint DEVICEQUERYREMOVEFAILED = 0x8002;
        //A request to remove a device or piece of media has been canceled.

        public const uint DEVICEREMOVECOMPLETE = 0x8004;
        //A device or piece of media has been removed.

        public const uint DEVICEREMOVEPENDING = 0x8003;
        //A device or piece of media is about to be removed. Cannot be denied.

        public const uint DEVICETYPESPECIFIC = 0x8005;
        //A device-specific event has occurred.

        public const uint DEVNODES_CHANGED = 0x0007;
        //A device has been added to or removed from the system.

        public const uint QUERYCHANGECONFIG = 0x0017;
        //Permission is requested to change the current configuration (dock or undock).

        public const uint USERDEFINED = 0xFFFF;


        public const int DEVTYP_OEM = 0;
        public const int DEVTYP_VOLUME = 2;
        public const int DEVTYP_PORT = 3;
        public const int DEVTYP_DEVICEINTERFACE = 5;

    }

    /// <summary>
    /// https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa363246(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DEV_BROADCAST_HDR
    {
        public uint Size;
        public uint DeviceType;
        public uint Reserved;
    }

    /// <summary>
    /// https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa363244(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int Size;
        public int DeviceType;
        public int Reserved;

        public Guid ClassGuid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string Name;
    }

    /// <summary>
    /// https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa363249(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DEV_BROADCAST_VOLUME
    {
        public int Size;
        public int DeviceType;
        public int Reserved;
        public int UnitMask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SP_DEVINFO_DATA
    {
        public UInt32 cbSize;
        public Guid ClassGuid;
        public UInt32 DevInst;
        public IntPtr Reserved;
    }

    [Flags]
    public enum DIGCF : int
    {
        DIGCF_DEFAULT = 0x00000001,    // only valid with DIGCF_DEVICEINTERFACE
        DIGCF_PRESENT = 0x00000002,
        DIGCF_ALLCLASSES = 0x00000004,
        DIGCF_PROFILE = 0x00000008,
        DIGCF_DEVICEINTERFACE = 0x00000010,
    }

    public enum SPDRP : int
    {
        SPDRP_DEVICEDESC = 0x00000000,
        SPDRP_HARDWAREID = 0x00000001,
        //...
    }

    public class KS
    {
        public static readonly Guid KSCATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");
        public static readonly Guid KSCATEGORY_VIDEO = new Guid("6994AD05-93EF-11D0-A3CC-00A0C9223196");
        public static readonly Guid KSCATEGORY_VIDEO_CAMERA = new Guid("E5323777-F976-4f5b-9B55-B94699C46E44");
    }


    public class WM
    {
        public const uint ACTIVATE = 0x0006;
        public const uint CLOSE = 0x0010;
        public const uint QUIT = 0x0012;

        public const uint DEVICECHANGE = 0x0219;
    }


    public static class AUDCLNT
    {   // AUDCLNT_ERR(n) MAKE_HRESULT(SEVERITY_ERROR, FACILITY_AUDCLNT, n)
        // AUDCLNT_SUCCESS(n) MAKE_SCODE(SEVERITY_SUCCESS, FACILITY_AUDCLNT, n)

        public const int SEVERITY_ERROR = 1;
        public const int FACILITY_AUDCLNT = 0x889;
        public static readonly int E_NOT_INITIALIZED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x001);
        public static readonly int E_ALREADY_INITIALIZED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x002);
        public static readonly int E_WRONG_ENDPOINT_TYPE = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x003);



        /// <summary>
        /// The audio endpoint device has been unplugged, 
        /// or the audio hardware or associated hardware resources have been reconfigured,
        /// disabled, removed, or otherwise made unavailable for use. 
        /// </summary>
        public static readonly int E_DEVICE_INVALIDATED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x004);

        public static readonly int E_NOT_STOPPED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x005);
        public static readonly int E_BUFFER_TOO_LARGE = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x006);
        public static readonly int E_OUT_OF_ORDER = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x007);
        public static readonly int E_UNSUPPORTED_FORMAT = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x008);
        public static readonly int E_INVALID_SIZE = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x009);
        public static readonly int E_DEVICE_IN_USE = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x00A);
        public static readonly int E_BUFFER_OPERATION_PENDING = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x00B);
        public static readonly int E_THREAD_NOT_REGISTERED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x00C);
        public static readonly int E_EXCLUSIVE_MODE_NOT_ALLOWED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x00E);
        public static readonly int E_ENDPOINT_CREATE_FAILED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x00F);
        public static readonly int E_SERVICE_NOT_RUNNING = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x010);
        public static readonly int E_EVENTHANDLE_NOT_EXPECTED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x011);
        public static readonly int E_EXCLUSIVE_MODE_ONLY = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x0012);
        public static readonly int E_BUFDURATION_PERIOD_NOT_EQUAL = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x013);
        public static readonly int E_EVENTHANDLE_NOT_SET = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x014);
        public static readonly int E_INCORRECT_BUFFER_SIZE = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x015);
        public static readonly int E_BUFFER_SIZE_ERROR = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x016);
        public static readonly int E_CPUUSAGE_EXCEEDED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x017);
        public static readonly int E_BUFFER_SIZE_NOT_ALIGNED = MakeHResult(SEVERITY_ERROR, FACILITY_AUDCLNT, 0x019);
        public static readonly int E_RESOURCES_INVALIDATED = unchecked((int)0x88890026);

        //static readonly int S_BUFFER_EMPTY              SUCCESS(0x001)
        //static readonly int S_THREAD_ALREADY_REGISTERED SUCCESS(0x002)
        //static readonly int S_POSITION_STALLED		   SUCCESS(0x003)

        public static int MakeHResult(uint ser, uint fac, uint code)
        {

            uint result = ser << 31;
            result |= fac << 16;
            result |= code;

            return unchecked((int)result);
        }

    }


}
