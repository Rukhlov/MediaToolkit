﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
    public unsafe partial class LibCuda
    {
        /// <summary>CUDA stream callback</summary>
        /// <param name="hStream">The stream the callback was added to, as passed to ::cuStreamAddCallback.  May be NULL.</param>
        /// <param name="status">::CUDA_SUCCESS or any persistent error on the stream.</param>
        /// <param name="userData">User parameter provided at registration.</param>
        public delegate void CuStreamCallback(CuStreamPtr hStream, CuResult status, IntPtr userData);

        /// <summary>Block size to per-block dynamic shared memory mapping for a certain kernel.</summary>
        /// <param name="blockSize">blockSize Block size of the kernel</param>
        /// <returns>The dynamic shared memory needed by a block.</returns>
        public delegate IntPtr CuOccupancyB2DSize(int blockSize);

        /// <summary>CUresult cuInit(unsigned int Flags)
        /// Initialize the CUDA driver API.</summary>
        [DllImport(nvCudaPath, EntryPoint = "cuInit")]
        public static extern CuResult Initialize(uint flags);

        /// <summary>Returns the CUDA driver version</summary>
        ///
        /// <remarks>
        /// Returns in *<paramref name="driverVersion"/> the version number of the installed CUDA
        /// driver. This function automatically returns ::CUDA_ERROR_INVALID_VALUE if
        /// the <paramref name="driverVersion"/> argument is NULL.
        /// </remarks>
        ///
        /// <param name="driverVersion">Returns the CUDA driver version</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_INVALID_VALUE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cudaDriverGetVersion,
        /// ::cudaRuntimeGetVersion
        /// CUresult CUDAAPI cuDriverGetVersion(int *driverVersion);
        [DllImport(nvCudaPath, EntryPoint = "cuDriverGetVersion")]
        public static extern CuResult DriverGetVersion(out int driverVersion);



        /// <summary>Gets the string description of an error code</summary>
        ///
        /// <remarks>
        /// Sets *<paramref name="str"/> to the address of a NULL-terminated string description
        /// of the error code <paramref name="error"/>.
        /// If the error code is not recognized, ::CUDA_ERROR_INVALID_VALUE
        /// will be returned and *<paramref name="str"/> will be set to the NULL address.
        /// </remarks>
        ///
        /// <param name="error">Error code to convert to string</param>
        /// <param name="str">Address of the string pointer.</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_INVALID_VALUE
        /// </returns>
        /// \sa
        /// ::CUresult,
        /// ::cudaGetErrorString
        /// CUresult CUDAAPI cuGetErrorString(CUresult error, const char **pStr);
        [DllImport(nvCudaPath, EntryPoint = "cuGetErrorString")]
        public static extern CuResult GetErrorString(CuResult error, out IntPtr str);

        /// <summary>Gets the string representation of an error code enum name</summary>
        ///
        /// <remarks>
        /// Sets *<paramref name="str"/> to the address of a NULL-terminated string representation
        /// of the name of the enum error code <c>error</c>.
        /// If the error code is not recognized, ::CUDA_ERROR_INVALID_VALUE
        /// will be returned and *<paramref name="str"/> will be set to the NULL address.
        /// </remarks>
        ///
        /// <param name="error">Error code to convert to string</param>
        /// <param name="str">Address of the string pointer.</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_INVALID_VALUE
        /// </returns>
        /// \sa
        /// ::CUresult,
        /// ::cudaGetErrorName
        /// CUresult CUDAAPI cuGetErrorName(CUresult error, const char **pStr);
        [DllImport(nvCudaPath, EntryPoint = "cuGetErrorName")]
        public static extern CuResult GetErrorName(CuResult error, out IntPtr str);


        /// CUresult CUDAAPI cuGetExportTable(const void **ppExportTable, const CUuuid *pExportTableId);
        [DllImport(nvCudaPath, EntryPoint = "cuGetExportTable")]
        public static extern CuResult GetExportTable(out IntPtr ppExportTable, out CUuuid pExportTableId);

    }
}
