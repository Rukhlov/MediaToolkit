using MediaToolkit.Codecs;
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

namespace Test.Decoder
{
	public partial class VideoDecoderPresenter
	{
		private void PresenterTaskRealTime(int presenterFps)
		{
			PerfCounter perfCounter = new PerfCounter();

			try
			{
				//presenterFps = 1;
				videoFrames = new BlockingCollection<VideoFrame>(1);

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


				double prevFrameTime = double.NaN;
				while (running)
				{
					while (videoFrames.Count > 0)
					{
						VideoFrame frame = null;
						try
						{

							bool frameTaken = videoFrames.TryTake(out frame, 1);
							if (!frameTaken)
							{
								Console.WriteLine("frameTaken == false");
								continue;
							}

							double frameDiff = 0;
							if (!double.IsNaN(prevFrameTime))
							{
								frameDiff = frame.time - prevFrameTime;
							}
							prevFrameTime = frame.time;

							if (frameDiff >= 0)
							{
								int delay = (int)(frameDiff * 1000);

								if (sourceReader.Count > 2)
								{
									delay = (int)(delay * 0.75);
								}
								else if (sourceReader.Count > 1)
								{
									delay = (int)(delay * 0.95);
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

								var cpuReport = perfCounter.GetReport();
								var timeNow = presentationClock.GetTime();
								int timeDelta = (int)((frame.time - timeNow) * 1000);
								int timeDelta2 = (int)((timeNow - frame.arrival) * 1000);

								var text = cpuReport + "\r\n"
									+ timeNow.ToString("0.000") + "\r\n"
									+ frame.time.ToString("0.000") + "\r\n"
									+ timeDelta + "\r\n"
									+ timeDelta2 + "\r\n"
									+ delay + "\r\n"
									+ frame.seq + "\r\n"
									+ sourceReader.Count + "\r\n"
									+ videoFrames.Count;


								presenter.Update(frame.tex, text);
							}
							else
							{
								Console.WriteLine("Non monotonic time: " + frame.time + "<" + prevFrameTime);
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

					//Console.WriteLine("syncEvent.WaitOne(10");
					syncEvent.WaitOne(10);
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


	}
}
