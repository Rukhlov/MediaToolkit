using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{
	public abstract class StatCounter
	{
		public abstract string GetReport();
		public abstract void Reset();
	}

	public class Statistic
	{

		public readonly static List<StatCounter> Stats = new List<StatCounter>();

		public static object syncRoot = new object();
		public static void RegisterCounter(StatCounter counter)
		{
			if (counter == null)
			{
				return;
			}

			lock (syncRoot)
			{
				Stats.Add(counter);
			}
		}

		public static void UnregisterCounter(StatCounter counter)
		{
			if (counter == null)
			{
				return;
			}

			lock (syncRoot)
			{
				Stats.Remove(counter);
			}
		}

		public static string GetReport()
		{
			string report = "";
			lock (syncRoot)
			{
				StringBuilder sb = new StringBuilder();
				foreach (var stat in Stats)
				{
					sb.AppendLine(stat.GetReport());
				}

				report = sb.ToString();

			}

			return report;
		}

		private static PerfCounter perfCounter = new PerfCounter();
		public static PerfCounter PerfCounter
		{
			get
			{
				if (perfCounter == null)
				{
					perfCounter = new PerfCounter();
				}
				return perfCounter;
			}
		}
	}



	public class PerfCounter : StatCounter, IDisposable
	{
		public PerfCounter()
		{
			_PerfCounter();
		}

		private void _PerfCounter()
		{
			timer.Interval = 1000;
			timer.Elapsed += Timer_Elapsed;
			timer.Disposed += Timer_Disposed;
			timer.Start();

		}

		public short CPU { get; private set; }

		private System.Timers.Timer timer = new System.Timers.Timer();
		private CPUCounter cpuCounter = new CPUCounter();

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			this.CPU = cpuCounter.GetUsage();
		}

		private void Timer_Disposed(object sender, EventArgs e)
		{
			cpuCounter?.Dispose();
		}

		public override string GetReport()
		{
			string cpuUsage = "";
			if (CPU >= 0 && CPU <= 100)
			{
				cpuUsage = "CPU=" + CPU + "%";
			}
			else
			{
				cpuUsage = "CPU=--%";
			}
			return cpuUsage;
		}

		public override void Reset()
		{
			//throw new NotImplementedException();
		}

		public void Dispose()
		{
			timer?.Stop();
			timer?.Dispose();
			timer = null;
		}

		class CPUCounter : IDisposable
		{

			private System.Runtime.InteropServices.ComTypes.FILETIME prevSysKernel;
			private System.Runtime.InteropServices.ComTypes.FILETIME prevSysUser;

			private TimeSpan prevProcTotal;

			private short CPUUsage;
			//DateTime LastRun;

			private long lastTimestamp;

			private long runCount;

			private Process currentProcess;

			public CPUCounter()
			{
				CPUUsage = -1;
				lastTimestamp = 0;

				prevSysUser.dwHighDateTime = prevSysUser.dwLowDateTime = 0;
				prevSysKernel.dwHighDateTime = prevSysKernel.dwLowDateTime = 0;
				prevProcTotal = TimeSpan.MinValue;
				runCount = 0;

				currentProcess = Process.GetCurrentProcess();
			}

			public short GetUsage()
			{
				if (disposed)
				{
					return 0;
				}

				short CPUCopy = CPUUsage;
				if (Interlocked.Increment(ref runCount) == 1)
				{
					if (!EnoughTimePassed)
					{
						Interlocked.Decrement(ref runCount);
						return CPUCopy;
					}

					System.Runtime.InteropServices.ComTypes.FILETIME sysIdle, sysKernel, sysUser;
					if (!Kernel32.GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
					{
						Interlocked.Decrement(ref runCount);
						return CPUCopy;
					}

					TimeSpan procTime = currentProcess.TotalProcessorTime;

					if (prevProcTotal != TimeSpan.MinValue)
					{
						ulong sysKernelDiff = SubtractTimes(sysKernel, prevSysKernel);
						ulong sysUserDiff = SubtractTimes(sysUser, prevSysUser);
						ulong sysTotal = sysKernelDiff + sysUserDiff;

						long procTotal = procTime.Ticks - prevProcTotal.Ticks;
						// long procTotal = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
						if (sysTotal > 0)
						{
							CPUUsage = (short)((100.0 * procTotal) / sysTotal);
						}
					}

					prevProcTotal = procTime;
					prevSysKernel = sysKernel;
					prevSysUser = sysUser;

					lastTimestamp = Stopwatch.GetTimestamp();

					CPUCopy = CPUUsage;
				}
				Interlocked.Decrement(ref runCount);

				return CPUCopy;

			}

			private ulong SubtractTimes(System.Runtime.InteropServices.ComTypes.FILETIME a, System.Runtime.InteropServices.ComTypes.FILETIME b)
			{
				ulong aInt = ((ulong)(a.dwHighDateTime << 32)) | (ulong)a.dwLowDateTime;
				ulong bInt = ((ulong)(b.dwHighDateTime << 32)) | (ulong)b.dwLowDateTime;

				return aInt - bInt;
			}

			private bool EnoughTimePassed
			{
				get
				{
					const int minimumElapsedMS = 250;

					long ticks = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
					TimeSpan sinceLast = new TimeSpan(ticks);

					return sinceLast.TotalMilliseconds > minimumElapsedMS;
				}
			}

			private volatile bool disposed = false;
			public void Dispose()
			{
				disposed = true;

				if (currentProcess != null)
				{
					currentProcess.Dispose();
					currentProcess = null;
				}
			}
		}

	}

}
