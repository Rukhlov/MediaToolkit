﻿using MediaToolkit.Codecs;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Probe
{
	public partial class VideoDecoderTest
	{
		private void PresenterTaskRealTime(int presenterFps)
		{
			PerfCounter perfCounter = new PerfCounter();

			try
			{
				//presenterFps = 1;
				videoFrames = new BlockingCollection<VideoFrame>(2);

				while (videoFrames.Count < videoFrames.BoundedCapacity)
				{
					Thread.Sleep(1);
					if (!running)
					{
						break;
					}
				}

				Console.WriteLine("videoQueue.IsAddingCompleted");
				presentationClock.Reset();
				//presentationClock.ClockRate = 0.997;
				AutoResetEvent syncEvent = new AutoResetEvent(false);

				VideoFrame frame = null;
				try
				{
					double prevFrameTime = double.NaN;
					while (running)
					{
						int delay = 0;

						while (videoFrames.Count > 0)
						{
							if (frame == null)
							{
								bool frameTaken = videoFrames.TryTake(out frame, 1);
								if (!frameTaken)
								{
									Console.WriteLine("frameTaken == false");
									continue;
								}
							}

							double delta = 0;
							if (!double.IsNaN(prevFrameTime))
							{
								delta = frame.time - prevFrameTime;
							}

							prevFrameTime = frame.time;

							if (delta < 0)
							{
								Console.WriteLine("Non monotonic time: " + frame.time + "<" + prevFrameTime);
								if (frame != null)
								{
									frame.Dispose();
									frame = null;
								}
								continue;
							}

							delay = (int)(delta * 1000);

		
							if (sourceReader.Count > 2)
							{
								delay = (int)(delay * 0.5);
							}
							else if (sourceReader.Count > 1)
							{
								delta = (int)(delay * 0.75);
							}

							if (delay > 0 && running)
							{
								if (delay > 3000)
								{
									Console.WriteLine(delay);
									delay = 3000;
								}

								syncEvent.WaitOne(delay);
							}

							try
							{
								var cpuReport = perfCounter.GetReport();
								var timeNow = presentationClock.GetTime();
								int timeDelta = (int)((frame.time - timeNow) * 1000);
								int timeDelta2 = (int)((timeNow - frame.arrival) * 1000);

								var text = cpuReport + "\r\n"
									+ timeNow.ToString("0.000") + "\r\n"
									+ frame.time.ToString("0.000") + "\r\n"
									+ timeDelta + "\r\n" 
									+ timeDelta2 + "\r\n"
									+ frame.seq + "\r\n"
									+ sourceReader.Count + "\r\n"
									+ videoFrames.Count;


								presenter.Update(frame.tex, text);
							}
							finally
							{
								if (frame != null)
								{
									frame.Dispose();
									frame = null;
								}
							}
						}

						syncEvent.WaitOne(10);
					}

				}
				finally
				{
					if (frame != null)
					{
						frame.Dispose();
						frame = null;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				running = false;
			}
			finally
			{
				if (videoFrames != null && videoFrames.Count > 0)
				{
					foreach (var f in videoFrames)
					{
						if (f != null)
						{
							f.Dispose();
						}
					}
					videoFrames.Dispose();
					videoFrames = null;
				}

				if (perfCounter != null)
				{
					perfCounter.Dispose();
					perfCounter = null;
				}
			}


		}


		class NalSourceReaderRealTime
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
			public double PacketInterval { get;  set; }
			public Task Start(string fileName, MfVideoArgs inputArgs)
			{
				if (running)
				{
					throw new InvalidOperationException("Invalid state " + running);
				}

				running = true;
				return Task.Run(() =>
				{
					//videoPackets = new Queue<VideoPacket>(4);
					videoPackets = new CircularQueue<VideoPacket>(16);

					Stream stream = null;
					try
					{
						var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
						PacketInterval = (double)frameRate[1] / frameRate[0];
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
										delay += rnd.Next(-5, 5);
										Thread.Sleep(delay);

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

	}
}