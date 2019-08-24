using System;
using System.Numerics;

namespace gravity_simulator_csharp
{
	internal class RandomSeedStrategy : ISeedStrategy
	{
		Random random = new Random(1);

		public Body NextBody(IUniverse universe)
		{
			const uint amplitude = 1500;
			const float velocity = 2;
			double tetha = random.NextDouble() * 2 * Math.PI;

			return new Body(
				mass: 1e12f,
				position: new Vector2((float)((random.NextDouble() - 0.5) * amplitude), (float)((random.NextDouble() - 0.5) * amplitude)),
				velocity: new Vector2((float)(Math.Cos(tetha) * velocity), (float)(Math.Sin(tetha) * velocity)),
				acceleration: Vector2.Zero
			);
		}
	}
}
