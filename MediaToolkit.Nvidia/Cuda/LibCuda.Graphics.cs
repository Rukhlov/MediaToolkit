﻿using System;
using System.Runtime.InteropServices;

namespace MediaToolkit.Nvidia
{
    public unsafe partial class LibCuda
    {
        /// <summary>Unregisters a graphics resource for access by CUDA
        ///
        /// Unregisters the graphics resource <paramref name="resource"/> so it is not accessible by
        /// CUDA unless registered again.
        ///
        /// If <paramref name="resource"/> is invalid then ::CUDA_ERROR_INVALID_HANDLE is
        /// returned.</summary>
        ///
        /// <param name="resource">Resource to unregister</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_UNKNOWN
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsD3D9RegisterResource,
        /// ::cuGraphicsD3D10RegisterResource,
        /// ::cuGraphicsD3D11RegisterResource,
        /// ::cuGraphicsGLRegisterBuffer,
        /// ::cuGraphicsGLRegisterImage,
        /// ::cudaGraphicsUnregisterResource
        /// CUresult CUDAAPI cuGraphicsUnregisterResource(CUgraphicsResource resource);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsUnregisterResource")]
        public static extern CuResult GraphicsUnregisterResource(CuGraphicsResource resource);

        /// <summary>Get an array through which to access a subresource of a mapped graphics resource.
        ///
        /// Returns in *<paramref name="pArray"/> an array through which the subresource of the mapped
        /// graphics resource <paramref name="resource"/> which corresponds to array index <paramref name="arrayIndex"/>
        /// and mipmap level <paramref name="mipLevel"/> may be accessed.  The value set in *<paramref name="pArray"/> may
        /// change every time that <paramref name="resource"/> is mapped.
        ///
        /// If <paramref name="resource"/> is not a texture then it cannot be accessed via an array and
        /// ::CUDA_ERROR_NOT_MAPPED_AS_ARRAY is returned.
        /// If <paramref name="arrayIndex"/> is not a valid array index for <paramref name="resource"/> then
        /// ::CUDA_ERROR_INVALID_VALUE is returned.
        /// If <paramref name="mipLevel"/> is not a valid mipmap level for <paramref name="resource"/> then
        /// ::CUDA_ERROR_INVALID_VALUE is returned.
        /// If <paramref name="resource"/> is not mapped then ::CUDA_ERROR_NOT_MAPPED is returned.</summary>
        ///
        /// <param name="pArray">Returned array through which a subresource of <paramref name="resource"/> may be accessed</param>
        /// <param name="resource">Mapped resource to access</param>
        /// <param name="arrayIndex">Array index for array textures or cubemap face
        /// index as defined by ::CUarray_cubemap_face for
        /// cubemap textures for the subresource to access</param>
        /// <param name="mipLevel">Mipmap level for the subresource to access</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_NOT_MAPPED,
        /// ::CUDA_ERROR_NOT_MAPPED_AS_ARRAY
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsResourceGetMappedPointer,
        /// ::cudaGraphicsSubResourceGetMappedArray
        /// CUresult CUDAAPI cuGraphicsSubResourceGetMappedArray(CUarray *pArray, CUgraphicsResource resource, unsigned int arrayIndex, unsigned int mipLevel);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsSubResourceGetMappedArray")]
        public static extern CuResult GraphicsSubResourceGetMappedArray(out CuArray pArray, CuGraphicsResource resource, int arrayIndex, int mipLevel);

        /// <summary>Get a mipmapped array through which to access a mapped graphics resource.
        ///
        /// Returns in *<paramref name="pMipmappedArray"/> a mipmapped array through which the mapped graphics
        /// resource <paramref name="resource"/>. The value set in *<paramref name="pMipmappedArray"/> may change every time
        /// that <paramref name="resource"/> is mapped.
        ///
        /// If <paramref name="resource"/> is not a texture then it cannot be accessed via a mipmapped array and
        /// ::CUDA_ERROR_NOT_MAPPED_AS_ARRAY is returned.
        /// If <paramref name="resource"/> is not mapped then ::CUDA_ERROR_NOT_MAPPED is returned.</summary>
        ///
        /// <param name="pMipmappedArray">Returned mipmapped array through which <paramref name="resource"/> may be accessed</param>
        /// <param name="resource">Mapped resource to access</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_NOT_MAPPED,
        /// ::CUDA_ERROR_NOT_MAPPED_AS_ARRAY
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsResourceGetMappedPointer,
        /// ::cudaGraphicsResourceGetMappedMipmappedArray
        /// CUresult CUDAAPI cuGraphicsResourceGetMappedMipmappedArray(CUmipmappedArray *pMipmappedArray, CUgraphicsResource resource);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsResourceGetMappedMipmappedArray")]
        public static extern CuResult GraphicsResourceGetMappedMipmappedArray(out CuMipMappedArray pMipmappedArray, CuGraphicsResource resource);

        /// <summary>Get a device pointer through which to access a mapped graphics resource.
        ///
        /// Returns in *<paramref name="pDevPtr"/> a pointer through which the mapped graphics resource
        /// <paramref name="resource"/> may be accessed.
        /// Returns in <paramref name="pSize"/> the size of the memory in bytes which may be accessed from that pointer.
        /// The value set in <c>pPointer</c> may change every time that <paramref name="resource"/> is mapped.
        ///
        /// If <paramref name="resource"/> is not a buffer then it cannot be accessed via a pointer and
        /// ::CUDA_ERROR_NOT_MAPPED_AS_POINTER is returned.
        /// If <paramref name="resource"/> is not mapped then ::CUDA_ERROR_NOT_MAPPED is returned.
        /// *</summary>
        ///
        /// <param name="pDevPtr">Returned pointer through which <paramref name="resource"/> may be accessed</param>
        /// <param name="pSize">Returned size of the buffer accessible starting at *<c>pPointer</c></param>
        /// <param name="resource">Mapped resource to access</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_NOT_MAPPED,
        /// ::CUDA_ERROR_NOT_MAPPED_AS_POINTER
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsMapResources,
        /// ::cuGraphicsSubResourceGetMappedArray,
        /// ::cudaGraphicsResourceGetMappedPointer
        /// CUresult CUDAAPI cuGraphicsResourceGetMappedPointer(CUdeviceptr *pDevPtr, size_t *pSize, CUgraphicsResource resource);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsResourceGetMappedPointer")]
        public static extern CuResult GraphicsResourceGetMappedPointer(out CuDevicePtr pDevPtr, out IntPtr pSize, CuGraphicsResource resource);

        /// <summary>Set usage flags for mapping a graphics resource</summary>
        ///
        /// <remarks>
        /// Set <paramref name="flags"/> for mapping the graphics resource <paramref name="resource"/>.
        ///
        /// Changes to <paramref name="flags"/> will take effect the next time <paramref name="resource"/> is mapped.
        /// The <paramref name="flags"/> argument may be any of the following:
        /// - ::CU_GRAPHICS_MAP_RESOURCE_FLAGS_NONE: Specifies no hints about how this
        ///   resource will be used. It is therefore assumed that this resource will be
        ///   read from and written to by CUDA kernels.  This is the default value.
        /// - ::CU_GRAPHICS_MAP_RESOURCE_FLAGS_READONLY: Specifies that CUDA kernels which
        ///   access this resource will not write to this resource.
        /// - ::CU_GRAPHICS_MAP_RESOURCE_FLAGS_WRITEDISCARD: Specifies that CUDA kernels
        ///   which access this resource will not read from this resource and will
        ///   write over the entire contents of the resource, so none of the data
        ///   previously stored in the resource will be preserved.
        ///
        /// If <paramref name="resource"/> is presently mapped for access by CUDA then
        /// ::CUDA_ERROR_ALREADY_MAPPED is returned.
        /// If <paramref name="flags"/> is not one of the above values then ::CUDA_ERROR_INVALID_VALUE is returned.
        /// </remarks>
        ///
        /// <param name="resource">Registered resource to set flags for</param>
        /// <param name="flags">Parameters for resource mapping</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_ALREADY_MAPPED
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsMapResources,
        /// ::cudaGraphicsResourceSetMapFlags
        /// CUresult CUDAAPI cuGraphicsResourceSetMapFlags(CUgraphicsResource resource, unsigned int flags);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsResourceSetMapFlags" + _ver)]
        public static extern CuResult GraphicsResourceSetMapFlags(CuGraphicsResource resource, CuGraphicsMapResources flags);

        /// <summary>Map graphics resources for access by CUDA
        ///
        /// Maps the <paramref name="count"/> graphics resources in <paramref name="resources"/> for access by CUDA.
        ///
        /// The resources in <paramref name="resources"/> may be accessed by CUDA until they
        /// are unmapped. The graphics API from which <paramref name="resources"/> were registered
        /// should not access any resources while they are mapped by CUDA. If an
        /// application does so, the results are undefined.
        ///
        /// This function provides the synchronization guarantee that any graphics calls
        /// issued before ::cuGraphicsMapResources() will complete before any subsequent CUDA
        /// work issued in <paramref name="hStream"/> begins.
        ///
        /// If <paramref name="resources"/> includes any duplicate entries then ::CUDA_ERROR_INVALID_HANDLE is returned.
        /// If any of <paramref name="resources"/> are presently mapped for access by CUDA then ::CUDA_ERROR_ALREADY_MAPPED is returned.</summary>
        ///
        /// <param name="count">Number of resources to map</param>
        /// <param name="resources">Resources to map for CUDA usage</param>
        /// <param name="hStream">Stream with which to synchronize</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_ALREADY_MAPPED,
        /// ::CUDA_ERROR_UNKNOWN
        /// \note_null_stream
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsResourceGetMappedPointer,
        /// ::cuGraphicsSubResourceGetMappedArray,
        /// ::cuGraphicsUnmapResources,
        /// ::cudaGraphicsMapResources
        /// CUresult CUDAAPI cuGraphicsMapResources(unsigned int count, CUgraphicsResource *resources, CUstream hStream);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsMapResources")]
        public static extern CuResult GraphicsMapResources(int count, CuGraphicsResource* resources, CuStream hStream);

        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsMapResources")]
        public static extern CuResult GraphicsMapResources(int count, IntPtr resources, CuStream hStream);


        /// <summary>Unmap graphics resources.
        ///
        /// Unmaps the <paramref name="count"/> graphics resources in <paramref name="resources"/>.
        ///
        /// Once unmapped, the resources in <paramref name="resources"/> may not be accessed by CUDA
        /// until they are mapped again.
        ///
        /// This function provides the synchronization guarantee that any CUDA work issued
        /// in <paramref name="hStream"/> before ::cuGraphicsUnmapResources() will complete before any
        /// subsequently issued graphics work begins.
        ///
        /// If <paramref name="resources"/> includes any duplicate entries then ::CUDA_ERROR_INVALID_HANDLE is returned.
        /// If any of <paramref name="resources"/> are not presently mapped for access by CUDA then ::CUDA_ERROR_NOT_MAPPED is returned.</summary>
        ///
        /// <param name="count">Number of resources to unmap</param>
        /// <param name="resources">Resources to unmap</param>
        /// <param name="hStream">Stream with which to synchronize</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_HANDLE,
        /// ::CUDA_ERROR_NOT_MAPPED,
        /// ::CUDA_ERROR_UNKNOWN
        /// \note_null_stream
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuGraphicsMapResources,
        /// ::cudaGraphicsUnmapResources
        /// CUresult CUDAAPI cuGraphicsUnmapResources(unsigned int count, CUgraphicsResource *resources, CUstream hStream);
        [DllImport(nvCudaPath, EntryPoint = "cuGraphicsUnmapResources")]
        public static extern CuResult GraphicsUnmapResources(int count, CuGraphicsResource* resources, CuStream hStream);
    }
}
