using System.Collections.Generic;

namespace gravity_simulator_csharp
{
	public interface IUniverse
	{
		float GravitationalConstant { get; }
		float OutwardBoundLimit { get; }

		uint ComputedTicks { get; }
		float Duration { get; }
		float ComputationsPerSecond { get; }

		IReadOnlyList<Body> Bodies { get; }
		int BodyCount { get; }

		void Tick();
	}
}
