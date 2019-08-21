using System;
using System.Numerics;

namespace gravity_simulator_csharp
{
	internal class RandomSeedStrategy : ISeedStrategy
	{
		Random random = new Random(1);

		public Body NextBody(Universe universe)
		{
			const uint amplitude = 1500;

			return new Body(
				mass: 1e12f,
				position: new Vector2((float)((random.NextDouble() - 0.5) * amplitude), (float)((random.NextDouble() - 0.5) * amplitude)),
				velocity: Vector2.Zero,
				acceleration: Vector2.Zero
			);
		}
	}
}
