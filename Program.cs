using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;

namespace gravity_simulator_csharp
{
    class Program
    {
        static void Main(string[] args)
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
			float percentTarget = 0.1f;

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

					if (percent >= percentTarget) {
						percentTarget += 0.1f;
						Console.Write("{0}%...", Math.Truncate(percent * 100));
					}
                }
            }

			watch.Stop();

			Console.WriteLine("");
			Console.WriteLine("Elapsed time: {0}", watch.Elapsed);
            OutputFrames(snapshots);
        }

        static void OutputFrames(SerializedUniverse universe)
        {
            Stream stream = new FileStream("output.json", FileMode.Create, FileAccess.Write, FileShare.None);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SerializedUniverse));
            serializer.WriteObject(stream, universe);
        }
    }
}
