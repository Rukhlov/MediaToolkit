using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using static MediaToolkit.Nvidia.LibNvEnc;

// ReSharper disable UnusedMember.Global

namespace MediaToolkit.Nvidia
{
    public class NvEncoder
    {
        private readonly NvEncoderPtr encoderPtr;
        internal NvEncoder(NvEncoderPtr ptr) 
        {
            this.encoderPtr = ptr;
        }

        public void GetEncodeGuidCount(out uint encodeGuidCount)
        {
            encodeGuidCount = 0;
            var status = NvEncApiFunc.GetEncodeGuidCount(encoderPtr, ref encodeGuidCount);
            CheckError(status);
        }

        public uint GetEncodeGuidCount()
        {
            uint encodeGuidCount = 0;
            var status = NvEncApiFunc.GetEncodeGuidCount(encoderPtr, ref encodeGuidCount);
            CheckError(status);
            return encodeGuidCount;
        }

        public unsafe void GetEncodeGuids(Guid[] guids, ref uint guidCount)
        {
            guidCount = 0;
            fixed (Guid* ptr = guids)
            {
                var status = NvEncApiFunc.GetEncodeGuids(encoderPtr, ptr, (uint)guids.Length, ref guidCount);
                CheckError(status);
            }
        }

        public IReadOnlyList<Guid> GetEncodeGuids()
        {
            var count = GetEncodeGuidCount();
            if (count == 0) 
            {
                return Array.Empty<Guid>();
            }
                
            Guid[] guids = new Guid[(int)count];

            GetEncodeGuids(guids, ref count);
            return guids;
        }

    
        public void GetEncodeProfileGuidCount(Guid encodeGuid, out uint encodeProfileGuidCount)
        {
            encodeProfileGuidCount = 0;
            var status = NvEncApiFunc.GetEncodeProfileGuidCount(encoderPtr, encodeGuid, ref encodeProfileGuidCount);
            CheckError(status);
        }

        public uint GetEncodeProfileGuidCount(Guid encodeGuid)
        {
            GetEncodeProfileGuidCount(encodeGuid, out var encodeProfileGuidCount);
            return encodeProfileGuidCount;
        }


        public unsafe void GetEncodeProfileGuids(Guid encodeGuid, Guid[] profileGuids, ref uint guidCount)
        {
            fixed (Guid* ptr = profileGuids)
            {
                var status = NvEncApiFunc.GetEncodeProfileGuids(encoderPtr, encodeGuid, ptr, (uint)profileGuids.Length, ref guidCount);
                CheckError(status);
            }
        }

        public IReadOnlyList<Guid> GetEncodeProfileGuids(Guid encodeGuid)
        {
            var count = GetEncodeProfileGuidCount(encodeGuid);
            if (count == 0)
            {
                return Array.Empty<Guid>();
            }
            Guid[] guids = new Guid[(int)count];

            GetEncodeProfileGuids(encodeGuid, guids, ref count);
            return guids;
        }


        public void GetInputFormatCount(Guid encodeGuid, out uint inputFmtCount)
        {
            inputFmtCount = 0;
            var status = NvEncApiFunc.GetInputFormatCount(encoderPtr, encodeGuid, ref inputFmtCount);
            CheckError(status);
        }

        public uint GetInputFormatCount(Guid encodeGuid)
        {
            uint inputFmtCount = 0;
            var status = NvEncApiFunc.GetInputFormatCount(encoderPtr, encodeGuid, ref inputFmtCount);
            CheckError(status);
            return inputFmtCount;
        }


        public unsafe void GetInputFormats(Guid encodeGuid, NvEncBufferFormat[] inputFmts, ref uint inputFmtCount)
        {
            fixed (NvEncBufferFormat* ptr = inputFmts)
            {
                var status = NvEncApiFunc.GetInputFormats(encoderPtr, encodeGuid, ptr, (uint)inputFmts.Length, ref inputFmtCount);
                CheckError(status);
            }
        }

        public IReadOnlyList<NvEncBufferFormat> GetInputFormats(Guid encodeGuid)
        {
            var count = GetInputFormatCount(encodeGuid);
            if (count == 0)
            {
                return Array.Empty<NvEncBufferFormat>();
            }

            NvEncBufferFormat[] inputFmts = new NvEncBufferFormat[(int)count];
            GetInputFormats(encodeGuid, inputFmts, ref count);

            return inputFmts;
        }


        public void GetEncodeCaps( Guid encodeGuid, ref NvEncCapsParam capsParam, ref int capsVal)
        {
            var status = NvEncApiFunc.GetEncodeCaps(encoderPtr, encodeGuid, ref capsParam, ref capsVal);

            CheckError(status);
        }

        public int GetEncodeCaps(Guid encodeGuig, NvEncCaps encCaps) 
        {
            NvEncCapsParam capsParam = new NvEncCapsParam
            {
                CapsToQuery = encCaps,
                Version = NvEncodeAPI.NV_ENC_CAPS_PARAM_VER,
            };

            int capsVal = 0;
            GetEncodeCaps(encodeGuig, ref capsParam, ref capsVal);
            return capsVal;
        }


        public void GetEncodePresetCount(Guid encodeGuid, out uint encodePresetGuidCount)
        {
            encodePresetGuidCount = 0;
            var status = NvEncApiFunc.GetEncodePresetCount(encoderPtr, encodeGuid, ref encodePresetGuidCount);
            CheckError(status);
        }

        public uint GetEncodePresetCount(Guid encodeGuid)
        {
            uint encodePresetGuidCount = 0;
            var status = NvEncApiFunc.GetEncodePresetCount(encoderPtr, encodeGuid, ref encodePresetGuidCount);
            CheckError(status);
            return encodePresetGuidCount;
        }

        public unsafe void GetEncodePresetGuids(Guid encodeGuid, Guid[] presetGuids, ref uint encodePresetGuidCount)
        {
            encodePresetGuidCount = 0;
            fixed (Guid* ptr = presetGuids)
            {
                var status = NvEncApiFunc.GetEncodePresetGuids(encoderPtr, encodeGuid, ptr, (uint)presetGuids.Length, ref encodePresetGuidCount);
                CheckError(status);
            }
        }

        public IReadOnlyList<Guid> GetEncodePresetGuids(Guid encodeGuid)
        {
            var count = GetEncodePresetCount(encodeGuid);
            if (count == 0)
            {
                return Array.Empty<Guid>();
            }

            Guid[] guids = new Guid[(int)count];
            GetEncodePresetGuids(encodeGuid, guids, ref count);
            return guids;
        }
   
        public void GetEncodePresetConfig( Guid encodeGuid, Guid presetGuid, ref NvEncPresetConfig presetConfig)
        {
            var status = NvEncApiFunc.GetEncodePresetConfig(encoderPtr, encodeGuid, presetGuid, ref presetConfig);
            CheckError(status);
        }

        public NvEncPresetConfig GetEncodePresetConfig(Guid encodeGuid, Guid presetGuid)
        {
            var presetConfig = new NvEncPresetConfig
            {
                Version = NvEncodeAPI.NV_ENC_PRESET_CONFIG_VER,
                PresetCfg = new NvEncConfig
                {
                    Version = NvEncodeAPI.NV_ENC_CONFIG_VER
                }
            };

            GetEncodePresetConfig(encodeGuid, presetGuid, ref presetConfig);
            return presetConfig;
        }

        public void InitializeEncoder(ref NvEncInitializeParams createEncodeParams)
        {
            var status = NvEncApiFunc.InitializeEncoder(encoderPtr, ref createEncodeParams);
            CheckError(status);
        }

      
        public void CreateInputBuffer(ref NvEncCreateInputBuffer createInputBufferParams)
        {
            var status = NvEncApiFunc.CreateInputBuffer(encoderPtr, ref createInputBufferParams);
            CheckError(status);
        }

        public NvEncCreateInputBuffer CreateInputBuffer(int width, int height, NvEncBufferFormat bufferFormat)
        {
            var createInputBufferParams = new NvEncCreateInputBuffer
            {
                Version = NvEncodeAPI.NV_ENC_CREATE_INPUT_BUFFER_VER,
                Width = (uint)width,
                Height = (uint)height,
                BufferFmt = bufferFormat
            };

            var status = NvEncApiFunc.CreateInputBuffer(encoderPtr, ref createInputBufferParams);
            CheckError(status);

            return createInputBufferParams;
        }

        
        public void DestroyInputBuffer(NvEncInputPtr inputBuffer)
        {
            var status = NvEncApiFunc.DestroyInputBuffer(encoderPtr, inputBuffer);
            CheckError(status);

        }


        public void SetIOCudaStreams(NvEncCustreamPtr inputStream, NvEncCustreamPtr outputStream)
        {
            var status = NvEncApiFunc.SetIOCudaStreams(encoderPtr, inputStream, outputStream);
            CheckError(status);
        }


        public void CreateBitstreamBuffer(ref NvEncCreateBitstreamBuffer createBitstreamBufferParams)
        {
            var status = NvEncApiFunc.CreateBitstreamBuffer(encoderPtr, ref createBitstreamBufferParams);
            CheckError(status);
        }

        public NvEncCreateBitstreamBuffer CreateBitstreamBuffer()
        {
            var createBitstreamBufferParams = new NvEncCreateBitstreamBuffer
            {
                Version = NvEncodeAPI.NV_ENC_CREATE_BITSTREAM_BUFFER_VER
            };

            var status = NvEncApiFunc.CreateBitstreamBuffer(encoderPtr, ref createBitstreamBufferParams);
            CheckError(status);

            return createBitstreamBufferParams;
        }


        public void DestroyBitstreamBuffer(NvEncOutputPtr bitstreamBuffer)
        {
            var status = NvEncApiFunc.DestroyBitstreamBuffer(encoderPtr, bitstreamBuffer);
            CheckError(status);
        }

       
        public void EncodePicture(ref NvEncPicParams encodePicParams)
        {
            var status = NvEncApiFunc.EncodePicture(encoderPtr, ref encodePicParams);

            CheckError(status);
        }

        public void LockBitstream(ref NvEncLockBitstream lockBitstreamBufferParams)
        {
            var status = NvEncApiFunc.LockBitstream(encoderPtr, ref lockBitstreamBufferParams);
            CheckError(status);
        }

        public NvEncLockBitstream LockBitstream( ref NvEncCreateBitstreamBuffer buffer, bool doNotWait = false)
        {
            var lockBitstreamBufferParams = new NvEncLockBitstream
            {
                Version = NvEncodeAPI.NV_ENC_LOCK_BITSTREAM_VER,
                OutputBitstream = buffer.BitstreamBuffer.Handle,
                DoNotWait = doNotWait
            };

            var status = NvEncApiFunc.LockBitstream(encoderPtr, ref lockBitstreamBufferParams);
            CheckError(status);

            return lockBitstreamBufferParams;
        }

        public NvEncLockedBitstream LockBitstreamAndCreateStream(ref NvEncCreateBitstreamBuffer buffer, bool doNotWait = false)
        {
            var locked = LockBitstream(ref buffer, doNotWait);

            return new NvEncLockedBitstream(this, buffer, locked);
        }

        public unsafe class NvEncLockedBitstream : UnmanagedMemoryStream
        {
            private readonly NvEncoder _encoder;
            private readonly NvEncCreateBitstreamBuffer _buffer;
            private int _disposed;

            public NvEncLockedBitstream(NvEncoder encoder, NvEncCreateBitstreamBuffer buffer, NvEncLockBitstream bitstream)
            : base((byte*)bitstream.BitstreamBufferPtr, bitstream.BitstreamSizeInBytes)
            {
                _encoder = encoder;
                _buffer = buffer;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing &&
                    Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    _encoder.UnlockBitstream(_buffer.BitstreamBuffer);
                }

                base.Dispose(disposing);
            }
        }


        public void UnlockBitstream(NvEncOutputPtr bitstreamBuffer)
        {
            var status = NvEncApiFunc.UnlockBitstream(encoderPtr, bitstreamBuffer);
            CheckError(status);
        }


        public void LockInputBuffer(ref NvEncLockInputBuffer lockInputBufferParams)
        {
            var status = NvEncApiFunc.LockInputBuffer(encoderPtr, ref lockInputBufferParams);
            CheckError(status);
        }

        public void UnlockInputBuffer(NvEncInputPtr inputBuffer)
        {
            var status = NvEncApiFunc.UnlockInputBuffer(encoderPtr, inputBuffer);

            CheckError(status);
        }


        public void GetEncodeStats(ref NvEncStat encodeStats)
        {
            var status = NvEncApiFunc.GetEncodeStats(encoderPtr, ref encodeStats);
            CheckError(status);
        }

        public void GetSequenceParams(ref NvEncSequenceParamPayload sequenceParamPayload)
        {
            var status = NvEncApiFunc.GetSequenceParams(encoderPtr, ref sequenceParamPayload);
            CheckError(status);
        }


        public void RegisterAsyncEvent(ref NvEncEventParams eventParams)
        {
            var status = NvEncApiFunc.RegisterAsyncEvent(encoderPtr, ref eventParams);
            CheckError(status);
        }


        public void UnregisterAsyncEvent(ref NvEncEventParams eventParams)
        {
            var status = NvEncApiFunc.UnregisterAsyncEvent(encoderPtr, ref eventParams);
            CheckError(status);
        }

        public void MapInputResource(ref NvEncMapInputResource mapInputResParams)
        {
            var status = NvEncApiFunc.MapInputResource(encoderPtr, ref mapInputResParams);
            CheckError(status);
        }

        public void UnmapInputResource(NvEncInputPtr mappedInputBuffer)
        {
            var status = NvEncApiFunc.UnmapInputResource(encoderPtr, mappedInputBuffer);
            CheckError(status);
        }

        public void DestroyEncoder()
        {
            var status = NvEncApiFunc.DestroyEncoder(encoderPtr);
            CheckError(status);
        }


        public void InvalidateRefFrames(ulong invalidRefFrameTimeStamp)
        {
            var status = NvEncApiFunc.InvalidateRefFrames(encoderPtr, invalidRefFrameTimeStamp);
            CheckError(status);
        }


        public NvEncRegisteredResource RegisterResource(ref NvEncRegisterResource registerResParams)
        {
            var status = NvEncApiFunc.RegisterResource(encoderPtr, ref registerResParams);

            CheckError(status);

            return new NvEncRegisteredResource(this, registerResParams.RegisteredResource);
        }

        public class NvEncRegisteredResource : IDisposable
        {
            private readonly NvEncoder _encoder;
            private readonly NvEncRegisteredPtr _resource;
            private int _disposed;

            public NvEncRegisteredResource(NvEncoder encoder, NvEncRegisteredPtr resource)
            {
                _encoder = encoder;
                _resource = resource;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

                _encoder.UnregisterResource(_resource);
            }
        }

        public void UnregisterResource(NvEncRegisteredPtr registeredResource)
        {
            var status = NvEncApiFunc.UnregisterResource(encoderPtr, registeredResource);
            CheckError(status);
        }


        public void ReconfigureEncoder(ref NvEncReconfigureParams reInitEncodeParams)
        {
            var status = NvEncApiFunc.ReconfigureEncoder(encoderPtr, ref reInitEncodeParams);
            CheckError(status);
        }

        public void CreateMvBuffer(ref NvEncCreateMvBuffer createMvBufferParams)
        {
            var status = NvEncApiFunc.CreateMvBuffer(encoderPtr, ref createMvBufferParams);
            CheckError(status);
        }


        public void DestroyMvBuffer(NvEncOutputPtr mvBuffer)
        {
            var status = NvEncApiFunc.DestroyMvBuffer(encoderPtr, mvBuffer);
            CheckError(status);
        }


        public void RunMotionEstimationOnly(ref NvEncMeonlyParams meOnlyParams)
        {
            var status = NvEncApiFunc.RunMotionEstimationOnly(encoderPtr, ref meOnlyParams);

            CheckError(status);
        }

        public void CheckError(NvEncStatus status)
        {
            if (status != NvEncStatus.Success)
            {
                var descr = GetLastError();

                throw new LibNvEncException("NvEncoder", descr, status);
            }
        }

        public string GetLastError()
        {
            if (encoderPtr.Handle == IntPtr.Zero) 
            {
                return "No NvEncoder.";
            }
                
            var ptr = NvEncApiFunc.GetLastError(encoderPtr);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }
    }
}
