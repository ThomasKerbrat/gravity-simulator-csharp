using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;

namespace gravity_simulator_csharp
{
	public class SimulationRecorder
	{
		private SerializedUniverse snapshots;

		public SimulationRecorder(IUniverse universe, float duration, float framesPerSecond)
		{
			Universe = universe;
			Duration = duration;
			FramesPerSecond = framesPerSecond;
			snapshots = new SerializedUniverse(universe.BodyCount, duration, framesPerSecond);
		}

		public IUniverse Universe { get; private set; }
		public float Duration { get; private set; }
		public float FramesPerSecond { get; private set; }

		public void Start()
		{
			snapshots.SnapshotUniverse(Universe);

			float durationBetweenFrames = 1f / FramesPerSecond;
			float durationSinceLastSnapshot = 0;
			float percent = 0;
			TimeSpan displayInterval = TimeSpan.FromSeconds(30);
			DateTime nextDisplay = DateTime.Now + displayInterval;

			Stopwatch watch = Stopwatch.StartNew();
			Console.WriteLine("{0} s ({1}%)\t{2} b\t{3} s", Math.Round(Universe.Duration, 2), Math.Floor(percent * 100), Universe.Bodies.Count, watch.Elapsed);

			while (Universe.Duration < Duration)
			{
				Universe.Tick();
				durationSinceLastSnapshot += 1f / Universe.ComputationsPerSecond;

				if (durationSinceLastSnapshot > durationBetweenFrames)
				{
					durationSinceLastSnapshot = 0;
					snapshots.SnapshotUniverse(Universe);

					percent = Universe.Duration / Duration;
				}

				if (DateTime.Now >= nextDisplay)
				{
					nextDisplay += displayInterval;
					Console.WriteLine("{0} s ({1}%)\t{2} b\t{3} s", Math.Round(Universe.Duration, 2), Math.Floor(percent * 100), Universe.Bodies.Count, watch.Elapsed);
				}
			}

			watch.Stop();

			Console.WriteLine("{0} s ({1}%)\t{2} b\t{3} s", Math.Round(Universe.Duration, 2), Math.Floor(percent * 100), Universe.Bodies.Count, watch.Elapsed);
			OutputFrames(snapshots);
		}

		private void OutputFrames(SerializedUniverse universe)
		{
			using (Stream stream = new FileStream("output.bin", FileMode.Create, FileAccess.Write, FileShare.None))
			{
				byte[] data = universe.Serialize();
				stream.Write(data, 0, data.Length);
			}
		}
	}
}
