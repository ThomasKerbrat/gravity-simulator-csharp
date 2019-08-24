
namespace gravity_simulator_csharp
{
	class Program
	{
		static void Main(string[] args)
		{
			const uint bodyCount = 1000;
			const float duration = 60;
			const float framesPerSecond = 30;
			const float computationsPerSecond = 100;

			IUniverse universe = new UniverseBarnesHut(computationsPerSecond, GalaxyStrategy.GetBodies(bodyCount));
			var recorder = new SimulationRecorder(universe, duration, framesPerSecond);

			recorder.Start();
		}
	}
}
