
namespace gravity_simulator_csharp
{
	class Program
	{
		static void Main(string[] args)
		{
			const uint bodyCount = 1024;
			const float duration = 10;
			const float framesPerSecond = 30;
			const float computationsPerSecond = 100;

			IUniverse universe = new UniverseBarnesHut(computationsPerSecond, GalaxyStrategy.GetBodies(bodyCount));
			var recorder = new SimulationRecorder(universe, duration, framesPerSecond);

			recorder.Start();
		}
	}
}
