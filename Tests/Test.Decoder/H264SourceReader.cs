using MediaToolkit.Codecs;
using MediaToolkit.MediaFoundation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Decoder
{
    interface INalSourceReader
    {
        bool PacketsAvailable { get; }
        bool IsFull { get; }
        int Count { get; }
        double PacketInterval { get; }
        bool TryGetPacket(out VideoPacket packet, int timeout);
        Task Start(string fileName, double interval);
        void Stop();

    }

    class NalSourceReaderRealTime : INalSourceReader
    {

        CircularQueue<VideoPacket> videoPackets = null;
        //private BlockingCollection<VideoPacket> videoPackets = null;
        private volatile bool running = false;
        public bool PacketsAvailable
        {
            get
            {
                bool available = false;
                if (videoPackets != null)
                {
                    available = videoPackets.Count > 0;
                }
                return available;
            }
        }

        public int Count
        {
            get
            {
                int count = -1;
                if (videoPackets != null)
                {
                    count = videoPackets.Count;
                }
                return count;
            }
        }
        public bool IsFull
        {
            get
            {
                bool isComplete = false;
                if (videoPackets != null)
                {
                    isComplete = videoPackets.IsComplete;
                }
                return isComplete;
            }
        }

        private object syncLock = new object();
        public bool TryGetPacket(out VideoPacket packet, int timeout)
        {
            packet = null;
            bool result = false;
            if (videoPackets != null)
            {
                //lock (syncLock)
                {
                    packet = videoPackets.Get();
                }

                result = true;
            }

            return result;
        }



        public int WaitDelay { get; set; } = 33;
        public double PacketInterval { get; set; }

        public Task Start(string fileName, double packetInterval)
        {
            if (running)
            {
                throw new InvalidOperationException("Invalid state " + running);
            }

            running = true;
            return Task.Run(() =>
            {
                //videoPackets = new Queue<VideoPacket>(4);
                videoPackets = new CircularQueue<VideoPacket>(64);

                Stream stream = null;
                try
                {
					//var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
					PacketInterval = packetInterval;//(double)frameRate[1] / frameRate[0];

					WaitDelay = (int)(PacketInterval * 1000);

                    long packetCount = 0;
                    double packetTime = 0;
                    double prevTime = 0;
                    stream = new FileStream(fileName, FileMode.Open);
                    var nalReader = new NalUnitReader(stream);
                    var dataAvailable = false;

                    bool loopback = true;
                    Stopwatch sw = new Stopwatch();
                    Random rnd = new Random();
                    while (loopback)
                    {
                        List<byte[]> nalsBuffer = new List<byte[]>();
                        do
                        {

                            //int delay = (int)(PacketInterval * 1000);
                            ////delay += rnd.Next(-5, 5);
                            //Thread.Sleep(delay);

                            dataAvailable = nalReader.ReadNext(out var nal);
                            if (nal != null && nal.Length > 0)
                            {
                                var firstByte = nal[0];
                                var nalUnitType = firstByte & 0x1F;


                                nalsBuffer.Add(nal);

                                if (nalUnitType == (int)NalUnitType.IDR || nalUnitType == (int)NalUnitType.Slice)
                                {
                                    IEnumerable<byte> data = new List<byte>();
                                    var startCodes = new byte[] { 0, 0, 0, 1 };
                                    foreach (var n in nalsBuffer)
                                    {
                                        data = data.Concat(startCodes).Concat(n);
                                    }

                                    nalsBuffer.Clear();
                                    packetTime = PacketInterval * packetCount;
                                    var bytes = data.ToArray();

                                    //  packetTime = sw.ElapsedMilliseconds / 1000.0;



                                    var packet = new VideoPacket
                                    {
                                        data = bytes,
                                        time = packetTime,
                                        duration = (packetTime - prevTime),
                                    };

                                    prevTime = packetTime;

                                    //Console.WriteLine("packetTime " + packet.time + " " + packet.duration);

                                    //videoPackets.Enqueue(packet);
                                    //lock (syncLock)
                                    {
                                        videoPackets.Add(packet);
                                    }


                                    int delay = (int)(PacketInterval * 1000);
                                    //delay += rnd.Next(-15, 15);
                                    Thread.Sleep(WaitDelay);

                                    packetCount++;
                                }
                                else if (nalUnitType == (int)NalUnitType.SequenceParameterSet)
                                {
                                    var rbsp = NalUnitReader.NalToRbsp(nal, 1);

                                    SequenceParameterSet.TryParse(rbsp, out var sps);
                                    Console.WriteLine(sps.ToString());

                                    //var startCodes = new byte[] { 0, 0, 0, 1 };
                                    //IEnumerable<byte> data = new List<byte>();
                                    //data = data.Concat(startCodes).Concat(nal);

                                    //File.WriteAllBytes(@"d:\temp\sps_1280x720_30fps.h264", data.ToArray());
                                }
                                else
                                {
                                    //Console.WriteLine("nalUnitType == " + nalUnitType);
                                }
                            }

                        } while (dataAvailable && running);

                        if (!running)
                        {
                            break;
                        }

                        stream.Position = 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    running = false;
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                    }

                    //if (videoPackets != null)
                    //{
                    //	videoPackets.Dispose();
                    //	videoPackets = null;
                    //}
                }
            });
        }

        public void Stop()
        {
            running = false;
        }

        public class CircularQueue<T> //where T : new()
        {
            private T[] buffer;

            private object locker = new object();

            private volatile int indexOut;
            private volatile int indexIn;

            private volatile int count;

            private volatile bool isComplete;

            public int Count => count;
            public bool IsComplete => isComplete;

            public readonly int Capacity = 8;
            public CircularQueue(int capacity = 8)
            {
                this.Capacity = capacity;
                buffer = new T[Capacity];
                indexOut = 0;
                indexIn = 0;
                count = 0;
            }

            public void Add(T t)
            {
                lock (locker)
                {
                    buffer[indexIn] = t;
                    indexIn = (indexIn + 1) % Capacity;
                    count = (count + 1) % Capacity;

                    isComplete = (indexIn == indexOut);

                    if (count == 0)
                    {
                        Console.WriteLine("Buffer full droping items...");
                    }
                }
            }


            public T Get()
            {
                T t = default(T);
                lock (locker)
                {
                    if (count > 0)
                    {
                        int index = indexOut;
                        indexOut = (indexOut + 1) % Capacity;
                        count = (count - 1) % Capacity;

                        isComplete = (indexIn == indexOut);

                        t = buffer[index];
                    }
                }

                return t;
            }
        }
    }

    class NalSourceReader : INalSourceReader
    {
        private BlockingCollection<VideoPacket> videoPackets = null;
        private volatile bool running = false;
        public bool PacketsAvailable
        {
            get
            {
                bool available = false;
                if (videoPackets != null)
                {
                    available = videoPackets.Count > 0;
                }
                return available;
            }
        }

        public bool IsFull
        {
            get
            {
                bool isAddingCompleted = false;
                if (videoPackets != null)
                {
                    isAddingCompleted = videoPackets.IsAddingCompleted;
                }
                return isAddingCompleted;
            }
        }
        public int Count
        {
            get
            {
                int count = -1;
                if (videoPackets != null)
                {
                    count = videoPackets.Count;
                }
                return count;
            }
        }


        public bool TryGetPacket(out VideoPacket packet, int timeout)
        {
            packet = null;
            bool result = false;
            if (videoPackets != null)
            {
                result = videoPackets.TryTake(out packet, timeout);
            }

            return result;
        }
        public double PacketInterval { get; private set; }
        public Task Start(string fileName, double interval)
        {
            if (running)
            {
                throw new InvalidOperationException("Invalid state " + running);
            }

            running = true;
            return Task.Run(() =>
            {
                //videoPackets = new Queue<VideoPacket>(4);
                videoPackets = new BlockingCollection<VideoPacket>(8);
                Stream stream = null;
                try
                {
					// var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
					PacketInterval = interval;//(double)frameRate[1] / frameRate[0];
					long packetCount = 0;
                    double packetTime = 0;

                    stream = new FileStream(fileName, FileMode.Open);
                    var nalReader = new NalUnitReader(stream);
                    var dataAvailable = false;

                    bool loopback = true;

                    Random rnd = new Random();
                    while (loopback)
                    {
                        List<byte[]> nalsBuffer = new List<byte[]>();
                        do
                        {
                            //int delay = (int)(sampleInterval * 1000);
                            //delay += rnd.Next(-5, 5);
                            //Thread.Sleep(delay);

                            dataAvailable = nalReader.ReadNext(out var nal);
                            if (nal != null && nal.Length > 0)
                            {
                                var firstByte = nal[0];
                                var nalUnitType = firstByte & 0x1F;
                                nalsBuffer.Add(nal);

                                if (nalUnitType == (int)NalUnitType.IDR || nalUnitType == (int)NalUnitType.Slice)
                                {
                                    IEnumerable<byte> data = new List<byte>();
                                    var startCodes = new byte[] { 0, 0, 0, 1 };
                                    foreach (var n in nalsBuffer)
                                    {
                                        data = data.Concat(startCodes).Concat(n);
                                    }

                                    nalsBuffer.Clear();
                                    packetTime = PacketInterval * packetCount;
                                    var bytes = data.ToArray();
                                    var packet = new VideoPacket
                                    {
                                        data = bytes,
                                        time = packetTime,
                                        duration = PacketInterval,
                                    };

                                    //videoPackets.Enqueue(packet);
                                    videoPackets.Add(packet);
                                    packetCount++;
                                }
                            }

                        } while (dataAvailable && running);

                        if (!running)
                        {
                            break;
                        }

                        stream.Position = 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    running = false;
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                        stream = null;
                    }

                    if (videoPackets != null)
                    {
                        videoPackets.Dispose();
                        videoPackets = null;
                    }
                }
            });
        }

        public void Stop()
        {
            running = false;
        }

    }

    class VideoPacket
    {
        public byte[] data = null;
        public double time = 0;
        public double duration = 0;
    }


}
