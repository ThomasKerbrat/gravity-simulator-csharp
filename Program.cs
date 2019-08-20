using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Json;

namespace gravity_simulator_csharp
{
	class Program
	{
		static void Main(string[] args)
		{
			Start();
		}

		static void Start()
		{
			const uint bodyNumber = 100;
			const uint simulationDurationInSeconds = 60;
			const uint framesPerSecond = 30;

			ISeedStrategy seedStrategy = new GlobularClusterStrategy();
			var universe = new Universe(computationsPerSecond: 100, bodyNumber, seedStrategy);
			var snapshots = new SerializedUniverse(bodyNumber, simulationDurationInSeconds, framesPerSecond);

			snapshots.SnapshotUniverse(universe);

			const float durationBetweenFrames = 1f / framesPerSecond;
			float durationSinceLastSnapshot = 0;
			float percent = 0;
			float percentTarget = 0.0f;

			Stopwatch watch = Stopwatch.StartNew();

			while (universe.Duration < simulationDurationInSeconds)
			{
				universe.Tick();
				durationSinceLastSnapshot += 1f / universe.ComputationsPerSecond;

				if (durationSinceLastSnapshot > durationBetweenFrames)
				{
					durationSinceLastSnapshot = 0;
					snapshots.SnapshotUniverse(universe);

					percent = universe.Duration / simulationDurationInSeconds;

					if (percent >= percentTarget)
					{
						percentTarget += 0.1f;
						Console.Write("{0}%...", Math.Truncate(percent * 100));
					}
				}
			}

			watch.Stop();

			Console.WriteLine("100%");
			Console.WriteLine("Elapsed time: {0}", watch.Elapsed);
			OutputFrames(snapshots);
		}

		static void OutputFrames(SerializedUniverse universe)
		{
			Stream stream = new FileStream("output.json", FileMode.Create, FileAccess.Write, FileShare.None);
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SerializedUniverse));
			serializer.WriteObject(stream, universe);
		}

		static void TestQuadTree()
		{
			var bodies = new List<Body>();
			var data = new float[]
			{
				1, -1, 1,
				1, 0.5f, 1.5f,
				1, 1.5f, 0.5f,
				1, 0.5f, -0.5f,
				1, 1.5f, -0.5f,
				1, 1.5f, -1.5f,
				1, -1, -1
			};

			for (int index = 0; index < data.Length; index += 3)
			{
				var body = new Body(data[index + 0], new Vector2(data[index + 1], data[index + 2]), Vector2.Zero, Vector2.Zero);
				bodies.Add(body);
			}

			var universe = new Universe(computationsPerSecond: 100, bodies);
			universe.Tick();

			Console.ReadLine();
		}
	}
}
