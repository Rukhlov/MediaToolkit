using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{

	//https://docs.microsoft.com/ru-ru/windows/win32/debug/system-error-codes
	public class Win32ErrorCodes
	{
		/// <summary>
		/// The operation completed successfully.
		/// </summary>
		public const int ERROR_SUCCESS = 0;
		/// <summary>
		/// Incorrect function.
		/// </summary>
		public const int ERROR_INVALID_FUNCTION = 1;
		/// <summary>
		/// The system cannot find the file specified.
		/// </summary>
		public const int ERROR_FILE_NOT_FOUND = 2;
		/// <summary>
		/// The system cannot find the path specified.
		/// </summary>
		public const int ERROR_PATH_NOT_FOUND = 3;
		/// <summary>
		/// The system cannot open the file.
		/// </summary>
		public const int ERROR_TOO_MANY_OPEN_FILES = 4;
		/// <summary>
		/// Access is denied.
		/// </summary>
		public const int ERROR_ACCESS_DENIED = 5;
		/// <summary>
		/// The handle is invalid.
		/// </summary>
		public const int ERROR_INVALID_HANDLE = 6;
		/// <summary>
		/// The storage control blocks were destroyed.
		/// </summary>
		public const int ERROR_ARENA_TRASHED = 7;
		/// <summary>
		/// Not enough storage is available to process this command.
		/// </summary>
		public const int ERROR_NOT_ENOUGH_MEMORY = 8;
		/// <summary>
		/// The storage control block address is invalid.
		/// </summary>
		public const int ERROR_INVALID_BLOCK = 9;
		/// <summary>
		/// The environment is incorrect.
		/// </summary>
		public const int ERROR_BAD_ENVIRONMENT = 10;
		/// <summary>
		/// An attempt was made to load a program with an incorrect format.
		/// </summary>
		public const int ERROR_BAD_FORMAT = 11;
		/// <summary>
		/// The access code is invalid.
		/// </summary>
		public const int ERROR_INVALID_ACCESS = 12;
		/// <summary>
		/// The data is invalid.
		/// </summary>
		public const int ERROR_INVALID_DATA = 13;
		/// <summary>
		/// Not enough storage is available to complete this operation.
		/// </summary>
		public const int ERROR_OUTOFMEMORY = 14;
		/// <summary>
		/// The system cannot find the drive specified.
		/// </summary>
		public const int ERROR_INVALID_DRIVE = 15;
		/// <summary>
		/// The directory cannot be removed.
		/// </summary>
		public const int ERROR_CURRENT_DIRECTORY = 16;
		/// <summary>
		/// The system cannot move the file to a different disk drive.
		/// </summary>
		public const int ERROR_NOT_SAME_DEVICE = 17;
		/// <summary>
		/// There are no more files.
		/// </summary>
		public const int ERROR_NO_MORE_FILES = 18;
		/// <summary>
		/// The media is write protected.
		/// </summary>
		public const int ERROR_WRITE_PROTECT = 19;
		/// <summary>
		/// The system cannot find the device specified.
		/// </summary>
		public const int ERROR_BAD_UNIT = 20;
		/// <summary>
		/// The device is not ready.
		/// </summary>
		public const int ERROR_NOT_READY = 21;
		/// <summary>
		/// The device does not recognize the command.
		/// </summary>
		public const int ERROR_BAD_COMMAND = 22;
		/// <summary>
		/// Data error (cyclic redundancy check).
		/// </summary>
		public const int ERROR_CRC = 23;
		/// <summary>
		/// The program issued a command but the command length is incorrect.
		/// </summary>
		public const int ERROR_BAD_LENGTH = 24;
		/// <summary>
		/// The drive cannot locate a specific area or track on the disk.
		/// </summary>
		public const int ERROR_SEEK = 25;
		/// <summary>
		/// The specified disk or diskette cannot be accessed.
		/// </summary>
		public const int ERROR_NOT_DOS_DISK = 26;
		/// <summary>
		/// The drive cannot find the sector requested.
		/// </summary>
		public const int ERROR_SECTOR_NOT_FOUND = 27;
		/// <summary>
		/// The printer is out of paper.
		/// </summary>
		public const int ERROR_OUT_OF_PAPER = 28;
		/// <summary>
		/// The system cannot write to the specified device.
		/// </summary>
		public const int ERROR_WRITE_FAULT = 29;
		/// <summary>
		/// The system cannot read from the specified device.
		/// </summary>
		public const int ERROR_READ_FAULT = 30;
		/// <summary>
		/// A device attached to the system is not functioning.
		/// </summary>
		public const int ERROR_GEN_FAILURE = 31;
		/// <summary>
		/// The process cannot access the file because it is being used by another process.
		/// </summary>
		public const int ERROR_SHARING_VIOLATION = 32;

		//...


		/// <summary>
		/// This operation returned because the timeout period expired.
		/// </summary>
		public const int ERROR_TIMEOUT = 1460;
	}

    public enum HResult
    {
        #region HRESULTs

        S_OK = 0,
        S_FALSE = 1,

        /// <summary>
        /// The data necessary to complete this operation is not yet available. 
        /// </summary>
        E_PENDING = unchecked((int)0x8000000A),

        /// <summary>Catastrophic failure</summary>
        E_UNEXPECTED = unchecked((int)0x8000FFFF),

        /// <summary>Not implemented</summary>
        E_NOTIMPL = unchecked((int)0x80004001),

        /// <summary>Ran out of memory</summary>
        E_OUTOFMEMORY = unchecked((int)0x8007000E),

        /// <summary>One or more arguments are invalid</summary>
        E_INVALIDARG = unchecked((int)0x80070057),

        /// <summary>No such interface supported</summary>
        E_NOINTERFACE = unchecked((int)0x80004002),

        /// <summary>Invalid pointer</summary>
        E_POINTER = unchecked((int)0x80004003),

        /// <summary>Invalid handle</summary>
        E_HANDLE = unchecked((int)0x80070006),

        /// <summary>Operation aborted</summary>
        E_ABORT = unchecked((int)0x80004004),

        /// <summary>Unspecified error</summary>
        E_FAIL = unchecked((int)0x80004005),

        /// <summary>General access denied error</summary>
        E_ACCESSDENIED = unchecked((int)0x80070005),

        #endregion

        #region Win32 HRESULTs

        /// <summary>The system cannot find the file specified.</summary>
        /// <unmanaged>HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND)</unmanaged>
        WIN32_ERROR_FILE_NOT_FOUND = unchecked((int)0x80070002),

        /// <summary>More data is available.</summary>
        /// <unmanaged>HRESULT_FROM_WIN32(ERROR_MORE_DATA)</unmanaged>
        WIN32_ERROR_MORE_DATA = unchecked((int)0x800700ea),

        /// <summary>No more data is available.</summary>
        /// <unmanaged>HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS)</unmanaged>
        WIN32_ERROR_NO_MORE_ITEMS = unchecked((int)0x80070103),

        /// <summary>Element not found.</summary>
        /// <unmanaged>HRESULT_FROM_WIN32(ERROR_NOT_FOUND)</unmanaged>
        WIN32_ERROR_NOT_FOUND = unchecked((int)0x80070490),

        /// <summary>
        /// he data area passed to a system call is too small. 
        /// </summary>
        WIN32_ERROR_INSUFFICIENT_BUFFER = unchecked((int)0x8007007a),
        #endregion

        #region Structured Storage HRESULTs

        /// <summary>The underlying file was converted to compound file format.</summary>
        STG_S_CONVERTED = unchecked((int)0x00030200),

        /// <summary>Multiple opens prevent consolidated. (commit succeeded).</summary>
        STG_S_MULTIPLEOPENS = unchecked((int)0x00030204),

        /// <summary>Consolidation of the storage file failed. (commit succeeded).</summary>
        STG_S_CONSOLIDATIONFAILED = unchecked((int)0x00030205),

        /// <summary>Consolidation of the storage file is inappropriate. (commit succeeded).</summary>
        STG_S_CANNOTCONSOLIDATE = unchecked((int)0x00030206),

        /// <summary>Unable to perform requested operation.</summary>
        STG_E_INVALIDFUNCTION = unchecked((int)0x80030001),

        /// <summary>The file could not be found.</summary>
        STG_E_FILENOTFOUND = unchecked((int)0x80030002),

        /// <summary>There are insufficient resources to open another file.</summary>
        STG_E_TOOMANYOPENFILES = unchecked((int)0x80030004),

        /// <summary>Access Denied.</summary>
        STG_E_ACCESSDENIED = unchecked((int)0x80030005),

        /// <summary>There is insufficient memory available to complete operation.</summary>
        STG_E_INSUFFICIENTMEMORY = unchecked((int)0x80030008),

        /// <summary>Invalid pointer error.</summary>
        STG_E_INVALIDPOINTER = unchecked((int)0x80030009),

        /// <summary>A disk error occurred during a write operation.</summary>
        STG_E_WRITEFAULT = unchecked((int)0x8003001D),

        /// <summary>A lock violation has occurred.</summary>
        STG_E_LOCKVIOLATION = unchecked((int)0x80030021),

        /// <summary>File already exists.</summary>
        STG_E_FILEALREADYEXISTS = unchecked((int)0x80030050),

        /// <summary>Invalid parameter error.</summary>
        STG_E_INVALIDPARAMETER = unchecked((int)0x80030057),

        /// <summary>There is insufficient disk space to complete operation.</summary>
        STG_E_MEDIUMFULL = unchecked((int)0x80030070),

        /// <summary>The name is not valid.</summary>
        STG_E_INVALIDNAME = unchecked((int)0x800300FC),

        /// <summary>Invalid flag error.</summary>
        STG_E_INVALIDFLAG = unchecked((int)0x800300FF),

        /// <summary>The storage has been changed since the last commit.</summary>
        STG_E_NOTCURRENT = unchecked((int)0x80030101),

        /// <summary>Attempted to use an object that has ceased to exist.</summary>
        STG_E_REVERTED = unchecked((int)0x80030102),

        /// <summary>Can't save.</summary>
        STG_E_CANTSAVE = unchecked((int)0x80030103),

        #endregion

        #region Various HRESULTs

        /// <summary>The function failed because the specified GDI device did not have any monitors associated with it.</summary>
        ERROR_GRAPHICS_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE = unchecked((int)0xC02625E5),

        #endregion

        #region Media Foundation HRESULTs

        MF_E_PLATFORM_NOT_INITIALIZED = unchecked((int)0xC00D36B0),

        MF_E_CAPTURE_ENGINE_ALL_EFFECTS_REMOVED = unchecked((int)0xC00DABE5),
        MF_E_CAPTURE_NO_SAMPLES_IN_QUEUE = unchecked((int)0xC00DABEB),
        MF_E_CAPTURE_PROPERTY_SET_DURING_PHOTO = unchecked((int)0xC00DABEA),
        MF_E_CAPTURE_SOURCE_DEVICE_EXTENDEDPROP_OP_IN_PROGRESS = unchecked((int)0xC00DABE9),
        MF_E_CAPTURE_SOURCE_NO_AUDIO_STREAM_PRESENT = unchecked((int)0xC00DABE8),
        MF_E_CAPTURE_SOURCE_NO_INDEPENDENT_PHOTO_STREAM_PRESENT = unchecked((int)0xC00DABE6),
        MF_E_CAPTURE_SOURCE_NO_VIDEO_STREAM_PRESENT = unchecked((int)0xC00DABE7),
        MF_E_HARDWARE_DRM_UNSUPPORTED = unchecked((int)0xC00D3706),
        MF_E_HDCP_AUTHENTICATION_FAILURE = unchecked((int)0xC00D7188),
        MF_E_HDCP_LINK_FAILURE = unchecked((int)0xC00D7189),
        MF_E_HW_ACCELERATED_THUMBNAIL_NOT_SUPPORTED = unchecked((int)0xC00DABEC),
        MF_E_NET_COMPANION_DRIVER_DISCONNECT = unchecked((int)0xC00D4295),
        MF_E_OPERATION_IN_PROGRESS = unchecked((int)0xC00D3705),
        MF_E_SINK_HEADERS_NOT_FOUND = unchecked((int)0xC00D4A45),
        MF_INDEX_SIZE_ERR = unchecked((int)0x80700001),
        MF_INVALID_ACCESS_ERR = unchecked((int)0x8070000F),
        MF_INVALID_STATE_ERR = unchecked((int)0x8070000B),
        MF_NOT_FOUND_ERR = unchecked((int)0x80700008),
        MF_NOT_SUPPORTED_ERR = unchecked((int)0x80700009),
        MF_PARSE_ERR = unchecked((int)0x80700051),
        MF_QUOTA_EXCEEDED_ERR = unchecked((int)0x80700016),
        MF_SYNTAX_ERR = unchecked((int)0x8070000C),

        MF_E_BUFFERTOOSMALL = unchecked((int)0xC00D36B1),

        MF_E_INVALIDREQUEST = unchecked((int)0xC00D36B2),
        MF_E_INVALIDSTREAMNUMBER = unchecked((int)0xC00D36B3),
        MF_E_INVALIDMEDIATYPE = unchecked((int)0xC00D36B4),
        MF_E_NOTACCEPTING = unchecked((int)0xC00D36B5),
        MF_E_NOT_INITIALIZED = unchecked((int)0xC00D36B6),
        MF_E_UNSUPPORTED_REPRESENTATION = unchecked((int)0xC00D36B7),
        MF_E_NO_MORE_TYPES = unchecked((int)0xC00D36B9),
        MF_E_UNSUPPORTED_SERVICE = unchecked((int)0xC00D36BA),
        MF_E_UNEXPECTED = unchecked((int)0xC00D36BB),
        MF_E_INVALIDNAME = unchecked((int)0xC00D36BC),
        MF_E_INVALIDTYPE = unchecked((int)0xC00D36BD),
        MF_E_INVALID_FILE_FORMAT = unchecked((int)0xC00D36BE),
        MF_E_INVALIDINDEX = unchecked((int)0xC00D36BF),
        MF_E_INVALID_TIMESTAMP = unchecked((int)0xC00D36C0),
        MF_E_UNSUPPORTED_SCHEME = unchecked((int)0xC00D36C3),
        MF_E_UNSUPPORTED_BYTESTREAM_TYPE = unchecked((int)0xC00D36C4),
        MF_E_UNSUPPORTED_TIME_FORMAT = unchecked((int)0xC00D36C5),
        MF_E_NO_SAMPLE_TIMESTAMP = unchecked((int)0xC00D36C8),
        MF_E_NO_SAMPLE_DURATION = unchecked((int)0xC00D36C9),
        MF_E_INVALID_STREAM_DATA = unchecked((int)0xC00D36CB),
        MF_E_RT_UNAVAILABLE = unchecked((int)0xC00D36CF),
        MF_E_UNSUPPORTED_RATE = unchecked((int)0xC00D36D0),
        MF_E_THINNING_UNSUPPORTED = unchecked((int)0xC00D36D1),
        MF_E_REVERSE_UNSUPPORTED = unchecked((int)0xC00D36D2),
        MF_E_UNSUPPORTED_RATE_TRANSITION = unchecked((int)0xC00D36D3),
        MF_E_RATE_CHANGE_PREEMPTED = unchecked((int)0xC00D36D4),
        MF_E_NOT_FOUND = unchecked((int)0xC00D36D5),
        MF_E_NOT_AVAILABLE = unchecked((int)0xC00D36D6),
        MF_E_NO_CLOCK = unchecked((int)0xC00D36D7),
        MF_S_MULTIPLE_BEGIN = unchecked((int)0x000D36D8),
        MF_E_MULTIPLE_BEGIN = unchecked((int)0xC00D36D9),
        MF_E_MULTIPLE_SUBSCRIBERS = unchecked((int)0xC00D36DA),
        MF_E_TIMER_ORPHANED = unchecked((int)0xC00D36DB),
        MF_E_STATE_TRANSITION_PENDING = unchecked((int)0xC00D36DC),
        MF_E_UNSUPPORTED_STATE_TRANSITION = unchecked((int)0xC00D36DD),
        MF_E_UNRECOVERABLE_ERROR_OCCURRED = unchecked((int)0xC00D36DE),
        MF_E_SAMPLE_HAS_TOO_MANY_BUFFERS = unchecked((int)0xC00D36DF),
        MF_E_SAMPLE_NOT_WRITABLE = unchecked((int)0xC00D36E0),
        MF_E_INVALID_KEY = unchecked((int)0xC00D36E2),
        MF_E_BAD_STARTUP_VERSION = unchecked((int)0xC00D36E3),
        MF_E_UNSUPPORTED_CAPTION = unchecked((int)0xC00D36E4),
        MF_E_INVALID_POSITION = unchecked((int)0xC00D36E5),
        MF_E_ATTRIBUTENOTFOUND = unchecked((int)0xC00D36E6),
        MF_E_PROPERTY_TYPE_NOT_ALLOWED = unchecked((int)0xC00D36E7),
        MF_E_PROPERTY_TYPE_NOT_SUPPORTED = unchecked((int)0xC00D36E8),
        MF_E_PROPERTY_EMPTY = unchecked((int)0xC00D36E9),
        MF_E_PROPERTY_NOT_EMPTY = unchecked((int)0xC00D36EA),
        MF_E_PROPERTY_VECTOR_NOT_ALLOWED = unchecked((int)0xC00D36EB),
        MF_E_PROPERTY_VECTOR_REQUIRED = unchecked((int)0xC00D36EC),
        MF_E_OPERATION_CANCELLED = unchecked((int)0xC00D36ED),
        MF_E_BYTESTREAM_NOT_SEEKABLE = unchecked((int)0xC00D36EE),
        MF_E_DISABLED_IN_SAFEMODE = unchecked((int)0xC00D36EF),
        MF_E_CANNOT_PARSE_BYTESTREAM = unchecked((int)0xC00D36F0),
        MF_E_SOURCERESOLVER_MUTUALLY_EXCLUSIVE_FLAGS = unchecked((int)0xC00D36F1),
        MF_E_MEDIAPROC_WRONGSTATE = unchecked((int)0xC00D36F2),
        MF_E_RT_THROUGHPUT_NOT_AVAILABLE = unchecked((int)0xC00D36F3),
        MF_E_RT_TOO_MANY_CLASSES = unchecked((int)0xC00D36F4),
        MF_E_RT_WOULDBLOCK = unchecked((int)0xC00D36F5),
        MF_E_NO_BITPUMP = unchecked((int)0xC00D36F6),
        MF_E_RT_OUTOFMEMORY = unchecked((int)0xC00D36F7),
        MF_E_RT_WORKQUEUE_CLASS_NOT_SPECIFIED = unchecked((int)0xC00D36F8),
        MF_E_INSUFFICIENT_BUFFER = unchecked((int)0xC00D7170),
        MF_E_CANNOT_CREATE_SINK = unchecked((int)0xC00D36FA),
        MF_E_BYTESTREAM_UNKNOWN_LENGTH = unchecked((int)0xC00D36FB),
        MF_E_SESSION_PAUSEWHILESTOPPED = unchecked((int)0xC00D36FC),
        MF_S_ACTIVATE_REPLACED = unchecked((int)0x000D36FD),
        MF_E_FORMAT_CHANGE_NOT_SUPPORTED = unchecked((int)0xC00D36FE),
        MF_E_INVALID_WORKQUEUE = unchecked((int)0xC00D36FF),
        MF_E_DRM_UNSUPPORTED = unchecked((int)0xC00D3700),
        MF_E_UNAUTHORIZED = unchecked((int)0xC00D3701),
        MF_E_OUT_OF_RANGE = unchecked((int)0xC00D3702),
        MF_E_INVALID_CODEC_MERIT = unchecked((int)0xC00D3703),
        MF_E_HW_MFT_FAILED_START_STREAMING = unchecked((int)0xC00D3704),
        MF_S_ASF_PARSEINPROGRESS = unchecked((int)0x400D3A98),
        MF_E_ASF_PARSINGINCOMPLETE = unchecked((int)0xC00D3A98),
        MF_E_ASF_MISSINGDATA = unchecked((int)0xC00D3A99),
        MF_E_ASF_INVALIDDATA = unchecked((int)0xC00D3A9A),
        MF_E_ASF_OPAQUEPACKET = unchecked((int)0xC00D3A9B),
        MF_E_ASF_NOINDEX = unchecked((int)0xC00D3A9C),
        MF_E_ASF_OUTOFRANGE = unchecked((int)0xC00D3A9D),
        MF_E_ASF_INDEXNOTLOADED = unchecked((int)0xC00D3A9E),
        MF_E_ASF_TOO_MANY_PAYLOADS = unchecked((int)0xC00D3A9F),
        MF_E_ASF_UNSUPPORTED_STREAM_TYPE = unchecked((int)0xC00D3AA0),
        MF_E_ASF_DROPPED_PACKET = unchecked((int)0xC00D3AA1),
        MF_E_NO_EVENTS_AVAILABLE = unchecked((int)0xC00D3E80),
        MF_E_INVALID_STATE_TRANSITION = unchecked((int)0xC00D3E82),
        MF_E_END_OF_STREAM = unchecked((int)0xC00D3E84),
        MF_E_SHUTDOWN = unchecked((int)0xC00D3E85),
        MF_E_MP3_NOTFOUND = unchecked((int)0xC00D3E86),
        MF_E_MP3_OUTOFDATA = unchecked((int)0xC00D3E87),
        MF_E_MP3_NOTMP3 = unchecked((int)0xC00D3E88),
        MF_E_MP3_NOTSUPPORTED = unchecked((int)0xC00D3E89),
        MF_E_NO_DURATION = unchecked((int)0xC00D3E8A),
        MF_E_INVALID_FORMAT = unchecked((int)0xC00D3E8C),
        MF_E_PROPERTY_NOT_FOUND = unchecked((int)0xC00D3E8D),
        MF_E_PROPERTY_READ_ONLY = unchecked((int)0xC00D3E8E),
        MF_E_PROPERTY_NOT_ALLOWED = unchecked((int)0xC00D3E8F),
        MF_E_MEDIA_SOURCE_NOT_STARTED = unchecked((int)0xC00D3E91),
        MF_E_UNSUPPORTED_FORMAT = unchecked((int)0xC00D3E98),
        MF_E_MP3_BAD_CRC = unchecked((int)0xC00D3E99),
        MF_E_NOT_PROTECTED = unchecked((int)0xC00D3E9A),
        MF_E_MEDIA_SOURCE_WRONGSTATE = unchecked((int)0xC00D3E9B),
        MF_E_MEDIA_SOURCE_NO_STREAMS_SELECTED = unchecked((int)0xC00D3E9C),
        MF_E_CANNOT_FIND_KEYFRAME_SAMPLE = unchecked((int)0xC00D3E9D),

        MF_E_UNSUPPORTED_CHARACTERISTICS = unchecked((int)0xC00D3E9E),
        MF_E_NO_AUDIO_RECORDING_DEVICE = unchecked((int)0xC00D3E9F),
        MF_E_AUDIO_RECORDING_DEVICE_IN_USE = unchecked((int)0xC00D3EA0),
        MF_E_AUDIO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA1),
        MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA2),
        MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED = unchecked((int)0xC00D3EA3),

        MF_E_NETWORK_RESOURCE_FAILURE = unchecked((int)0xC00D4268),
        MF_E_NET_WRITE = unchecked((int)0xC00D4269),
        MF_E_NET_READ = unchecked((int)0xC00D426A),
        MF_E_NET_REQUIRE_NETWORK = unchecked((int)0xC00D426B),
        MF_E_NET_REQUIRE_ASYNC = unchecked((int)0xC00D426C),
        MF_E_NET_BWLEVEL_NOT_SUPPORTED = unchecked((int)0xC00D426D),
        MF_E_NET_STREAMGROUPS_NOT_SUPPORTED = unchecked((int)0xC00D426E),
        MF_E_NET_MANUALSS_NOT_SUPPORTED = unchecked((int)0xC00D426F),
        MF_E_NET_INVALID_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D4270),
        MF_E_NET_CACHESTREAM_NOT_FOUND = unchecked((int)0xC00D4271),
        MF_I_MANUAL_PROXY = unchecked((int)0x400D4272),
        MF_E_NET_REQUIRE_INPUT = unchecked((int)0xC00D4274),
        MF_E_NET_REDIRECT = unchecked((int)0xC00D4275),
        MF_E_NET_REDIRECT_TO_PROXY = unchecked((int)0xC00D4276),
        MF_E_NET_TOO_MANY_REDIRECTS = unchecked((int)0xC00D4277),
        MF_E_NET_TIMEOUT = unchecked((int)0xC00D4278),
        MF_E_NET_CLIENT_CLOSE = unchecked((int)0xC00D4279),
        MF_E_NET_BAD_CONTROL_DATA = unchecked((int)0xC00D427A),
        MF_E_NET_INCOMPATIBLE_SERVER = unchecked((int)0xC00D427B),
        MF_E_NET_UNSAFE_URL = unchecked((int)0xC00D427C),
        MF_E_NET_CACHE_NO_DATA = unchecked((int)0xC00D427D),
        MF_E_NET_EOL = unchecked((int)0xC00D427E),
        MF_E_NET_BAD_REQUEST = unchecked((int)0xC00D427F),
        MF_E_NET_INTERNAL_SERVER_ERROR = unchecked((int)0xC00D4280),
        MF_E_NET_SESSION_NOT_FOUND = unchecked((int)0xC00D4281),
        MF_E_NET_NOCONNECTION = unchecked((int)0xC00D4282),
        MF_E_NET_CONNECTION_FAILURE = unchecked((int)0xC00D4283),
        MF_E_NET_INCOMPATIBLE_PUSHSERVER = unchecked((int)0xC00D4284),
        MF_E_NET_SERVER_ACCESSDENIED = unchecked((int)0xC00D4285),
        MF_E_NET_PROXY_ACCESSDENIED = unchecked((int)0xC00D4286),
        MF_E_NET_CANNOTCONNECT = unchecked((int)0xC00D4287),
        MF_E_NET_INVALID_PUSH_TEMPLATE = unchecked((int)0xC00D4288),
        MF_E_NET_INVALID_PUSH_PUBLISHING_POINT = unchecked((int)0xC00D4289),
        MF_E_NET_BUSY = unchecked((int)0xC00D428A),
        MF_E_NET_RESOURCE_GONE = unchecked((int)0xC00D428B),
        MF_E_NET_ERROR_FROM_PROXY = unchecked((int)0xC00D428C),
        MF_E_NET_PROXY_TIMEOUT = unchecked((int)0xC00D428D),
        MF_E_NET_SERVER_UNAVAILABLE = unchecked((int)0xC00D428E),
        MF_E_NET_TOO_MUCH_DATA = unchecked((int)0xC00D428F),
        MF_E_NET_SESSION_INVALID = unchecked((int)0xC00D4290),
        MF_E_OFFLINE_MODE = unchecked((int)0xC00D4291),
        MF_E_NET_UDP_BLOCKED = unchecked((int)0xC00D4292),
        MF_E_NET_UNSUPPORTED_CONFIGURATION = unchecked((int)0xC00D4293),
        MF_E_NET_PROTOCOL_DISABLED = unchecked((int)0xC00D4294),
        MF_E_ALREADY_INITIALIZED = unchecked((int)0xC00D4650),
        MF_E_BANDWIDTH_OVERRUN = unchecked((int)0xC00D4651),
        MF_E_LATE_SAMPLE = unchecked((int)0xC00D4652),
        MF_E_FLUSH_NEEDED = unchecked((int)0xC00D4653),
        MF_E_INVALID_PROFILE = unchecked((int)0xC00D4654),
        MF_E_INDEX_NOT_COMMITTED = unchecked((int)0xC00D4655),
        MF_E_NO_INDEX = unchecked((int)0xC00D4656),
        MF_E_CANNOT_INDEX_IN_PLACE = unchecked((int)0xC00D4657),
        MF_E_MISSING_ASF_LEAKYBUCKET = unchecked((int)0xC00D4658),
        MF_E_INVALID_ASF_STREAMID = unchecked((int)0xC00D4659),
        MF_E_STREAMSINK_REMOVED = unchecked((int)0xC00D4A38),
        MF_E_STREAMSINKS_OUT_OF_SYNC = unchecked((int)0xC00D4A3A),
        MF_E_STREAMSINKS_FIXED = unchecked((int)0xC00D4A3B),
        MF_E_STREAMSINK_EXISTS = unchecked((int)0xC00D4A3C),
        MF_E_SAMPLEALLOCATOR_CANCELED = unchecked((int)0xC00D4A3D),
        MF_E_SAMPLEALLOCATOR_EMPTY = unchecked((int)0xC00D4A3E),
        MF_E_SINK_ALREADYSTOPPED = unchecked((int)0xC00D4A3F),
        MF_E_ASF_FILESINK_BITRATE_UNKNOWN = unchecked((int)0xC00D4A40),
        MF_E_SINK_NO_STREAMS = unchecked((int)0xC00D4A41),
        MF_S_SINK_NOT_FINALIZED = unchecked((int)0x000D4A42),
        MF_E_METADATA_TOO_LONG = unchecked((int)0xC00D4A43),
        MF_E_SINK_NO_SAMPLES_PROCESSED = unchecked((int)0xC00D4A44),
        MF_E_VIDEO_REN_NO_PROCAMP_HW = unchecked((int)0xC00D4E20),
        MF_E_VIDEO_REN_NO_DEINTERLACE_HW = unchecked((int)0xC00D4E21),
        MF_E_VIDEO_REN_COPYPROT_FAILED = unchecked((int)0xC00D4E22),
        MF_E_VIDEO_REN_SURFACE_NOT_SHARED = unchecked((int)0xC00D4E23),
        MF_E_VIDEO_DEVICE_LOCKED = unchecked((int)0xC00D4E24),
        MF_E_NEW_VIDEO_DEVICE = unchecked((int)0xC00D4E25),
        MF_E_NO_VIDEO_SAMPLE_AVAILABLE = unchecked((int)0xC00D4E26),
        MF_E_NO_AUDIO_PLAYBACK_DEVICE = unchecked((int)0xC00D4E84),
        MF_E_AUDIO_PLAYBACK_DEVICE_IN_USE = unchecked((int)0xC00D4E85),
        MF_E_AUDIO_PLAYBACK_DEVICE_INVALIDATED = unchecked((int)0xC00D4E86),
        MF_E_AUDIO_SERVICE_NOT_RUNNING = unchecked((int)0xC00D4E87),
        MF_E_TOPO_INVALID_OPTIONAL_NODE = unchecked((int)0xC00D520E),
        MF_E_TOPO_CANNOT_FIND_DECRYPTOR = unchecked((int)0xC00D5211),
        MF_E_TOPO_CODEC_NOT_FOUND = unchecked((int)0xC00D5212),
        MF_E_TOPO_CANNOT_CONNECT = unchecked((int)0xC00D5213),
        MF_E_TOPO_UNSUPPORTED = unchecked((int)0xC00D5214),
        MF_E_TOPO_INVALID_TIME_ATTRIBUTES = unchecked((int)0xC00D5215),
        MF_E_TOPO_LOOPS_IN_TOPOLOGY = unchecked((int)0xC00D5216),
        MF_E_TOPO_MISSING_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D5217),
        MF_E_TOPO_MISSING_STREAM_DESCRIPTOR = unchecked((int)0xC00D5218),
        MF_E_TOPO_STREAM_DESCRIPTOR_NOT_SELECTED = unchecked((int)0xC00D5219),
        MF_E_TOPO_MISSING_SOURCE = unchecked((int)0xC00D521A),
        MF_E_TOPO_SINK_ACTIVATES_UNSUPPORTED = unchecked((int)0xC00D521B),
        MF_E_SEQUENCER_UNKNOWN_SEGMENT_ID = unchecked((int)0xC00D61AC),
        MF_S_SEQUENCER_CONTEXT_CANCELED = unchecked((int)0x000D61AD),
        MF_E_NO_SOURCE_IN_CACHE = unchecked((int)0xC00D61AE),
        MF_S_SEQUENCER_SEGMENT_AT_END_OF_STREAM = unchecked((int)0x000D61AF),
        MF_E_TRANSFORM_TYPE_NOT_SET = unchecked((int)0xC00D6D60),
        MF_E_TRANSFORM_STREAM_CHANGE = unchecked((int)0xC00D6D61),
        MF_E_TRANSFORM_INPUT_REMAINING = unchecked((int)0xC00D6D62),
        MF_E_TRANSFORM_PROFILE_MISSING = unchecked((int)0xC00D6D63),
        MF_E_TRANSFORM_PROFILE_INVALID_OR_CORRUPT = unchecked((int)0xC00D6D64),
        MF_E_TRANSFORM_PROFILE_TRUNCATED = unchecked((int)0xC00D6D65),
        MF_E_TRANSFORM_PROPERTY_PID_NOT_RECOGNIZED = unchecked((int)0xC00D6D66),
        MF_E_TRANSFORM_PROPERTY_VARIANT_TYPE_WRONG = unchecked((int)0xC00D6D67),
        MF_E_TRANSFORM_PROPERTY_NOT_WRITEABLE = unchecked((int)0xC00D6D68),
        MF_E_TRANSFORM_PROPERTY_ARRAY_VALUE_WRONG_NUM_DIM = unchecked((int)0xC00D6D69),
        MF_E_TRANSFORM_PROPERTY_VALUE_SIZE_WRONG = unchecked((int)0xC00D6D6A),
        MF_E_TRANSFORM_PROPERTY_VALUE_OUT_OF_RANGE = unchecked((int)0xC00D6D6B),
        MF_E_TRANSFORM_PROPERTY_VALUE_INCOMPATIBLE = unchecked((int)0xC00D6D6C),
        MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_OUTPUT_MEDIATYPE = unchecked((int)0xC00D6D6D),
        MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_INPUT_MEDIATYPE = unchecked((int)0xC00D6D6E),
        MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_MEDIATYPE_COMBINATION = unchecked((int)0xC00D6D6F),
        MF_E_TRANSFORM_CONFLICTS_WITH_OTHER_CURRENTLY_ENABLED_FEATURES = unchecked((int)0xC00D6D70),
        MF_E_TRANSFORM_NEED_MORE_INPUT = unchecked((int)0xC00D6D72),
        MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_SPKR_CONFIG = unchecked((int)0xC00D6D73),
        MF_E_TRANSFORM_CANNOT_CHANGE_MEDIATYPE_WHILE_PROCESSING = unchecked((int)0xC00D6D74),
        MF_S_TRANSFORM_DO_NOT_PROPAGATE_EVENT = unchecked((int)0x000D6D75),
        MF_E_UNSUPPORTED_D3D_TYPE = unchecked((int)0xC00D6D76),
        MF_E_TRANSFORM_ASYNC_LOCKED = unchecked((int)0xC00D6D77),
        MF_E_TRANSFORM_CANNOT_INITIALIZE_ACM_DRIVER = unchecked((int)0xC00D6D78L),
        MF_E_LICENSE_INCORRECT_RIGHTS = unchecked((int)0xC00D7148),
        MF_E_LICENSE_OUTOFDATE = unchecked((int)0xC00D7149),
        MF_E_LICENSE_REQUIRED = unchecked((int)0xC00D714A),
        MF_E_DRM_HARDWARE_INCONSISTENT = unchecked((int)0xC00D714B),
        MF_E_NO_CONTENT_PROTECTION_MANAGER = unchecked((int)0xC00D714C),
        MF_E_LICENSE_RESTORE_NO_RIGHTS = unchecked((int)0xC00D714D),
        MF_E_BACKUP_RESTRICTED_LICENSE = unchecked((int)0xC00D714E),
        MF_E_LICENSE_RESTORE_NEEDS_INDIVIDUALIZATION = unchecked((int)0xC00D714F),
        MF_S_PROTECTION_NOT_REQUIRED = unchecked((int)0x000D7150),
        MF_E_COMPONENT_REVOKED = unchecked((int)0xC00D7151),
        MF_E_TRUST_DISABLED = unchecked((int)0xC00D7152),
        MF_E_WMDRMOTA_NO_ACTION = unchecked((int)0xC00D7153),
        MF_E_WMDRMOTA_ACTION_ALREADY_SET = unchecked((int)0xC00D7154),
        MF_E_WMDRMOTA_DRM_HEADER_NOT_AVAILABLE = unchecked((int)0xC00D7155),
        MF_E_WMDRMOTA_DRM_ENCRYPTION_SCHEME_NOT_SUPPORTED = unchecked((int)0xC00D7156),
        MF_E_WMDRMOTA_ACTION_MISMATCH = unchecked((int)0xC00D7157),
        MF_E_WMDRMOTA_INVALID_POLICY = unchecked((int)0xC00D7158),
        MF_E_POLICY_UNSUPPORTED = unchecked((int)0xC00D7159),
        MF_E_OPL_NOT_SUPPORTED = unchecked((int)0xC00D715A),
        MF_E_TOPOLOGY_VERIFICATION_FAILED = unchecked((int)0xC00D715B),
        MF_E_SIGNATURE_VERIFICATION_FAILED = unchecked((int)0xC00D715C),
        MF_E_DEBUGGING_NOT_ALLOWED = unchecked((int)0xC00D715D),
        MF_E_CODE_EXPIRED = unchecked((int)0xC00D715E),
        MF_E_GRL_VERSION_TOO_LOW = unchecked((int)0xC00D715F),
        MF_E_GRL_RENEWAL_NOT_FOUND = unchecked((int)0xC00D7160),
        MF_E_GRL_EXTENSIBLE_ENTRY_NOT_FOUND = unchecked((int)0xC00D7161),
        MF_E_KERNEL_UNTRUSTED = unchecked((int)0xC00D7162),
        MF_E_PEAUTH_UNTRUSTED = unchecked((int)0xC00D7163),
        MF_E_NON_PE_PROCESS = unchecked((int)0xC00D7165),
        MF_E_REBOOT_REQUIRED = unchecked((int)0xC00D7167),
        MF_S_WAIT_FOR_POLICY_SET = unchecked((int)0x000D7168),
        MF_S_VIDEO_DISABLED_WITH_UNKNOWN_SOFTWARE_OUTPUT = unchecked((int)0x000D7169),
        MF_E_GRL_INVALID_FORMAT = unchecked((int)0xC00D716A),
        MF_E_GRL_UNRECOGNIZED_FORMAT = unchecked((int)0xC00D716B),
        MF_E_ALL_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716C),
        MF_E_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716D),
        MF_E_USERMODE_UNTRUSTED = unchecked((int)0xC00D716E),
        MF_E_PEAUTH_SESSION_NOT_STARTED = unchecked((int)0xC00D716F),
        MF_E_PEAUTH_PUBLICKEY_REVOKED = unchecked((int)0xC00D7171),
        MF_E_GRL_ABSENT = unchecked((int)0xC00D7172),
        MF_S_PE_TRUSTED = unchecked((int)0x000D7173),
        MF_E_PE_UNTRUSTED = unchecked((int)0xC00D7174),
        MF_E_PEAUTH_NOT_STARTED = unchecked((int)0xC00D7175),
        MF_E_INCOMPATIBLE_SAMPLE_PROTECTION = unchecked((int)0xC00D7176),
        MF_E_PE_SESSIONS_MAXED = unchecked((int)0xC00D7177),
        MF_E_HIGH_SECURITY_LEVEL_CONTENT_NOT_ALLOWED = unchecked((int)0xC00D7178),
        MF_E_TEST_SIGNED_COMPONENTS_NOT_ALLOWED = unchecked((int)0xC00D7179),
        MF_E_ITA_UNSUPPORTED_ACTION = unchecked((int)0xC00D717A),
        MF_E_ITA_ERROR_PARSING_SAP_PARAMETERS = unchecked((int)0xC00D717B),
        MF_E_POLICY_MGR_ACTION_OUTOFBOUNDS = unchecked((int)0xC00D717C),
        MF_E_BAD_OPL_STRUCTURE_FORMAT = unchecked((int)0xC00D717D),
        MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_PROTECTION_GUID = unchecked((int)0xC00D717E),
        MF_E_NO_PMP_HOST = unchecked((int)0xC00D717F),
        MF_E_ITA_OPL_DATA_NOT_INITIALIZED = unchecked((int)0xC00D7180),
        MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_OUTPUT = unchecked((int)0xC00D7181),
        MF_E_ITA_UNRECOGNIZED_DIGITAL_VIDEO_OUTPUT = unchecked((int)0xC00D7182),

        MF_E_RESOLUTION_REQUIRES_PMP_CREATION_CALLBACK = unchecked((int)0xC00D7183),
        MF_E_INVALID_AKE_CHANNEL_PARAMETERS = unchecked((int)0xC00D7184),
        MF_E_CONTENT_PROTECTION_SYSTEM_NOT_ENABLED = unchecked((int)0xC00D7185),
        MF_E_UNSUPPORTED_CONTENT_PROTECTION_SYSTEM = unchecked((int)0xC00D7186),
        MF_E_DRM_MIGRATION_NOT_SUPPORTED = unchecked((int)0xC00D7187),

        MF_E_CLOCK_INVALID_CONTINUITY_KEY = unchecked((int)0xC00D9C40),
        MF_E_CLOCK_NO_TIME_SOURCE = unchecked((int)0xC00D9C41),
        MF_E_CLOCK_STATE_ALREADY_SET = unchecked((int)0xC00D9C42),
        MF_E_CLOCK_NOT_SIMPLE = unchecked((int)0xC00D9C43),
        MF_S_CLOCK_STOPPED = unchecked((int)0x000D9C44),
        MF_E_NO_MORE_DROP_MODES = unchecked((int)0xC00DA028),
        MF_E_NO_MORE_QUALITY_LEVELS = unchecked((int)0xC00DA029),
        MF_E_DROPTIME_NOT_SUPPORTED = unchecked((int)0xC00DA02A),
        MF_E_QUALITYKNOB_WAIT_LONGER = unchecked((int)0xC00DA02B),
        MF_E_QM_INVALIDSTATE = unchecked((int)0xC00DA02C),
        MF_E_TRANSCODE_NO_CONTAINERTYPE = unchecked((int)0xC00DA410),
        MF_E_TRANSCODE_PROFILE_NO_MATCHING_STREAMS = unchecked((int)0xC00DA411),
        MF_E_TRANSCODE_NO_MATCHING_ENCODER = unchecked((int)0xC00DA412),

        MF_E_TRANSCODE_INVALID_PROFILE = unchecked((int)0xC00DA413),

        MF_E_ALLOCATOR_NOT_INITIALIZED = unchecked((int)0xC00DA7F8),
        MF_E_ALLOCATOR_NOT_COMMITED = unchecked((int)0xC00DA7F9),
        MF_E_ALLOCATOR_ALREADY_COMMITED = unchecked((int)0xC00DA7FA),
        MF_E_STREAM_ERROR = unchecked((int)0xC00DA7FB),
        MF_E_INVALID_STREAM_STATE = unchecked((int)0xC00DA7FC),
        MF_E_HW_STREAM_NOT_CONNECTED = unchecked((int)0xC00DA7FD),

        MF_E_NO_CAPTURE_DEVICES_AVAILABLE = unchecked((int)0xC00DABE0),
        MF_E_CAPTURE_SINK_OUTPUT_NOT_SET = unchecked((int)0xC00DABE1),
        MF_E_CAPTURE_SINK_MIRROR_ERROR = unchecked((int)0xC00DABE2),
        MF_E_CAPTURE_SINK_ROTATE_ERROR = unchecked((int)0xC00DABE3),
        MF_E_CAPTURE_ENGINE_INVALID_OP = unchecked((int)0xC00DABE4),

        MF_E_DXGI_DEVICE_NOT_INITIALIZED = unchecked((int)0x80041000),
        MF_E_DXGI_NEW_VIDEO_DEVICE = unchecked((int)0x80041001),
        MF_E_DXGI_VIDEO_DEVICE_LOCKED = unchecked((int)0x80041002),

        #endregion
    }

    public static class Defines
    {
        //HWND_MESSAGE
        public static readonly IntPtr HWndMessage = new IntPtr(-3);

        //HWND_DESKTOP
        public static readonly IntPtr HWndDesktop = IntPtr.Zero;

        //MAX_PATH
        public const int MaxPath = 260;

        //HWND_BROADCAST
        public const int HWndBroadcast = 0xFFFF;

        //...
    }



    public static class WM
    {
        public const int HTCLIENT = 1;
        public const int HTCAPTION = 2;
        public const int HTMENU = 5;
        public const int HTMAXBUTTON = 9;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int HTCLOSE = 0x20;


        public const int ACTIVATE = 0x0006;
        public const int CLOSE = 0x0010;
        public const int QUIT = 0x0012;


        public const int NCHITTEST = 0x84;
        public const int NCLBUTTONDOWN = 0xA1;

        public const int DEVICECHANGE = 0x0219;

		public const int WTSSESSION_CHANGE = 0x02B1;

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

    public static class WS 
    {
        public const int Overlapped = 0x00000000;
        public const int MaximizeBox = 0x00010000;
    }

    public static class WS_EX 
    {
        public const int Layered = 0x80000;
        public const int Composited = 0x02000000;
        public const int ToolWindow = 0x00000080;// 
        public const int TopMost = 0x00000008;
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

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;

        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
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
    public static class DBT
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

    public enum QUERY_DEVICE_CONFIG_FLAGS : uint
    {
        QDC_ALL_PATHS = 0x00000001,
        QDC_ONLY_ACTIVE_PATHS = 0x00000002,
        QDC_DATABASE_CURRENT = 0x00000004
    }

    public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint
    {
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = 0xFFFFFFFF,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = 0x80000000,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
    {
        DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
        DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
        DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_ROTATION : uint
    {
        DISPLAYCONFIG_ROTATION_IDENTITY = 1,
        DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
        DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
        DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
        DISPLAYCONFIG_ROTATION_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_SCALING : uint
    {
        DISPLAYCONFIG_SCALING_IDENTITY = 1,
        DISPLAYCONFIG_SCALING_CENTERED = 2,
        DISPLAYCONFIG_SCALING_STRETCHED = 3,
        DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
        DISPLAYCONFIG_SCALING_CUSTOM = 5,
        DISPLAYCONFIG_SCALING_PREFERRED = 128,
        DISPLAYCONFIG_SCALING_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_PIXELFORMAT : uint
    {
        DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
        DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
        DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
        DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
        DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
        DISPLAYCONFIG_PIXELFORMAT_FORCE_UINT32 = 0xffffffff
    }

    public enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
    {
        DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
        DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
        DISPLAYCONFIG_MODE_INFO_TYPE_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
    {
        DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
        DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
        DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
        DISPLAYCONFIG_DEVICE_INFO_FORCE_UINT32 = 0xFFFFFFFF
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        private DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
        private DISPLAYCONFIG_ROTATION rotation;
        private DISPLAYCONFIG_SCALING scaling;
        private DISPLAYCONFIG_RATIONAL refreshRate;
        private DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        public bool targetAvailable;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_RATIONAL
    {
        public uint Numerator;
        public uint Denominator;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_INFO
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_2DREGION
    {
        public uint cx;
        public uint cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
    {
        public ulong pixelRate;
        public DISPLAYCONFIG_RATIONAL hSyncFreq;
        public DISPLAYCONFIG_RATIONAL vSyncFreq;
        public DISPLAYCONFIG_2DREGION activeSize;
        public DISPLAYCONFIG_2DREGION totalSize;
        public uint videoStandard;
        public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_MODE
    {
        public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SOURCE_MODE
    {
        public uint width;
        public uint height;
        public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
        public POINTL position;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DISPLAYCONFIG_MODE_INFO_UNION
    {
        [FieldOffset(0)]
        public DISPLAYCONFIG_TARGET_MODE targetMode;

        [FieldOffset(0)]
        public DISPLAYCONFIG_SOURCE_MODE sourceMode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_MODE_INFO
    {
        public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
        public uint id;
        public LUID adapterId;
        public DISPLAYCONFIG_MODE_INFO_UNION modeInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
    {
        public uint value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
        public uint size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DISPLAYCONFIG_TARGET_DEVICE_NAME : DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
        public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint connectorInstance;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string monitorFriendlyDeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string monitorDevicePath;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DISPLAYCONFIG_SOURCE_DEVICE_NAME: DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        private const int Cchdevicename = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Cchdevicename)]
        public string viewGdiDeviceName;
    }

    public static class KS
    {
        public static readonly Guid CATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");
        public static readonly Guid CATEGORY_VIDEO = new Guid("6994AD05-93EF-11D0-A3CC-00A0C9223196");
		public static readonly Guid CATEGORY_AUDIO = new Guid("6994AD04-93EF-11D0-A3CC-00A0C9223196");

		public static readonly Guid CATEGORY_VIDEO_CAMERA = new Guid("E5323777-F976-4f5b-9B55-B94699C46E44");
    }

	public static class GUID
	{
		public static readonly Guid DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
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


	public class USEROBJECTFLAGS
	{
		public int fInherit = 0;
		public int fReserved = 0;
		public int dwFlags = 0;
	}




	public enum MonitorState
	{
		MonitorStateOn = -1,
		MonitorStateOff = 2,
		MonitorStateStandBy = 1
	}

	[Flags]
	public enum ACCESS_MASK : uint
	{
		DELETE = 0x00010000,
		READ_CONTROL = 0x00020000,
		WRITE_DAC = 0x00040000,
		WRITE_OWNER = 0x00080000,
		SYNCHRONIZE = 0x00100000,

		STANDARD_RIGHTS_REQUIRED = 0x000F0000,

		STANDARD_RIGHTS_READ = 0x00020000,
		STANDARD_RIGHTS_WRITE = 0x00020000,
		STANDARD_RIGHTS_EXECUTE = 0x00020000,

		STANDARD_RIGHTS_ALL = 0x001F0000,

		SPECIFIC_RIGHTS_ALL = 0x0000FFFF,

		ACCESS_SYSTEM_SECURITY = 0x01000000,

		MAXIMUM_ALLOWED = 0x02000000,

		GENERIC_READ = 0x80000000,
		GENERIC_WRITE = 0x40000000,
		GENERIC_EXECUTE = 0x20000000,
		GENERIC_ALL = 0x10000000,

		DESKTOP_READOBJECTS = 0x00000001,
		DESKTOP_CREATEWINDOW = 0x00000002,
		DESKTOP_CREATEMENU = 0x00000004,
		DESKTOP_HOOKCONTROL = 0x00000008,
		DESKTOP_JOURNALRECORD = 0x00000010,
		DESKTOP_JOURNALPLAYBACK = 0x00000020,
		DESKTOP_ENUMERATE = 0x00000040,
		DESKTOP_WRITEOBJECTS = 0x00000080,
		DESKTOP_SWITCHDESKTOP = 0x00000100,

		WINSTA_ENUMDESKTOPS = 0x00000001,
		WINSTA_READATTRIBUTES = 0x00000002,
		WINSTA_ACCESSCLIPBOARD = 0x00000004,
		WINSTA_CREATEDESKTOP = 0x00000008,
		WINSTA_WRITEATTRIBUTES = 0x00000010,
		WINSTA_ACCESSGLOBALATOMS = 0x00000020,
		WINSTA_EXITWINDOWS = 0x00000040,
		WINSTA_ENUMERATE = 0x00000100,
		WINSTA_READSCREEN = 0x00000200,

		WINSTA_ALL_ACCESS = 0x0000037F
	}




	public class PrivilegeConstants
	{

		public const int SE_PRIVILEGE_ENABLED = 0x00000002;
		public const int SE_PRIVILEGE_DISABLED = 0x00000000;
		public const int SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
		public const int SE_PRIVILEGE_REMOVED = 0x00000004;


		public const string SE_ASSIGNPRIMARYTOKEN_NAME = "SeAssignPrimaryTokenPrivilege";
		public const string SE_AUDIT_NAME = "SeAuditPrivilege";
		public const string SE_BACKUP_NAME = "SeBackupPrivilege";
		public const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
		public const string SE_CREATE_GLOBAL_NAME = "SeCreateGlobalPrivilege";
		public const string SE_CREATE_PAGEFILE_NAME = "SeCreatePagefilePrivilege";
		public const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
		public const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
		public const string SE_CREATE_TOKEN_NAME = "SeCreateTokenPrivilege";
		public const string SE_DEBUG_NAME = "SeDebugPrivilege";
		public const string SE_DELEGATE_SESSION_USER_IMPERSONATE_NAME = "SeDelegateSessionUserImpersonatePrivilege";
		public const string SE_ENABLE_DELEGATION_NAME = "SeEnableDelegationPrivilege";
		public const string SE_IMPERSONATE_NAME = "SeImpersonatePrivilege";
		public const string SE_INC_BASE_PRIORITY_NAME = "SeIncreaseBasePriorityPrivilege";
		public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
		public const string SE_INC_WORKING_SET_NAME = "SeIncreaseWorkingSetPrivilege";
		public const string SE_LOAD_DRIVER_NAME = "SeLoadDriverPrivilege";
		public const string SE_LOCK_MEMORY_NAME = "SeLockMemoryPrivilege";
		public const string SE_MACHINE_ACCOUNT_NAME = "SeMachineAccountPrivilege";
		public const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
		public const string SE_PROF_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
		public const string SE_RELABEL_NAME = "SeRelabelPrivilege";
		public const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";
		public const string SE_RESTORE_NAME = "SeRestorePrivilege";
		public const string SE_SECURITY_NAME = "SeSecurityPrivilege";
		public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
		public const string SE_SYNC_AGENT_NAME = "SeSyncAgentPrivilege";
		public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
		public const string SE_SYSTEM_PROFILE_NAME = "SeSystemProfilePrivilege";
		public const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
		public const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
		public const string SE_TCB_NAME = "SeTcbPrivilege";
		public const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
		public const string SE_TRUSTED_CREDMAN_ACCESS_NAME = "SeTrustedCredManAccessPrivilege";
		public const string SE_UNDOCK_NAME = "SeUndockPrivilege";
	}
}
