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

			ISeedStrategy seedStrategy = new PlanetRingStrategy();
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
			var origin = new Rectangle(new System.Numerics.Vector2(-2, -2), 4, 4);
			var tree = new BarnesHutTree(origin, 2);

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

			var bodies = new List<Body>();

			for (int index = 0; index < data.Length; index += 3)
			{
				var body = new Body(data[index + 0], new Vector2(data[index + 1], data[index + 2]), Vector2.Zero, Vector2.Zero);
				bodies.Add(body);
				tree.Insert(body);
			}

			List<VirtualBody> result = tree.Query(bodies[5], 0.75f);

			Console.ReadLine();
		}
	}
}
