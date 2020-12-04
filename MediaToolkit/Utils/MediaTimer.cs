using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{

	public class MediaTimer
	{
		public const long TicksPerMillisecond = 10000;
		public const long TicksPerSecond = TicksPerMillisecond * 1000;

		public static double GetRelativeTimeMilliseconds()
		{
			return (Ticks / (double)TicksPerMillisecond);
		}

		public static double GetRelativeTime()
		{
			return (Ticks / (double)TicksPerSecond);
		}

		public static long Ticks
		{
			get
			{
				return (long)(Stopwatch.GetTimestamp() * TicksPerSecond / (double)Stopwatch.Frequency);
				//return DateTime.Now.Ticks;
				//return NativeMethods.timeGetTime() * TicksPerMillisecond;
			}
		}

		public static DateTime GetDateTimeFromNtpTimestamp(ulong ntmTimestamp)
		{
			uint TimestampMSW = (uint)(ntmTimestamp >> 32);
			uint TimestampLSW = (uint)(ntmTimestamp & 0x00000000ffffffff);

			return GetDateTimeFromNtpTimestamp(TimestampMSW, TimestampLSW);
		}

		public static DateTime GetDateTimeFromNtpTimestamp(uint TimestampMSW, uint TimestampLSW)
		{
			/*
            Timestamp, MSW: 3670566484 (0xdac86654)
            Timestamp, LSW: 3876982392 (0xe7160e78)
            [MSW and LSW as NTP timestamp: Apr 25, 2016 09:48:04.902680000 UTC]
             * */

			DateTime ntpDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			uint ntpTimeMilliseconds = (uint)(Math.Round((double)TimestampLSW / (double)uint.MaxValue, 3) * 1000);
			return ntpDateTime
				.AddSeconds(TimestampMSW)
				.AddMilliseconds(ntpTimeMilliseconds);
		}


		private DateTime startDateTime;
		private long startTimestamp;
		private bool isRunning = false;

		public void Start(DateTime dateTime)
		{
			if (isRunning == false)
			{
				startDateTime = dateTime;
				startTimestamp = Stopwatch.GetTimestamp();

				isRunning = true;
			}
		}

		public DateTime Now
		{
			get
			{
				DateTime dateTime = DateTime.MinValue;
				if (isRunning)
				{
					dateTime = startDateTime.AddTicks(ElapsedTicks);
				}

				return dateTime;
			}
		}

		public TimeSpan Elapsed
		{
			get
			{
				TimeSpan timeSpan = TimeSpan.Zero;
				if (isRunning)
				{
					timeSpan = new TimeSpan(ElapsedTicks);
				}
				return timeSpan;
			}
		}

		public long ElapsedTicks
		{
			get
			{
				long ticks = 0;
				if (isRunning)
				{
					ticks = (long)((Stopwatch.GetTimestamp() - startTimestamp) * TicksPerSecond / (double)Stopwatch.Frequency);

					if (ticks < 0)
					{
						//...
					}
				}
				return ticks;
			}
		}

		public void Stop()
		{

			if (isRunning)
			{
				isRunning = false;
			}

		}

	}

}
