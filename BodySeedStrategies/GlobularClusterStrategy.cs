using System;
using System.Numerics;

namespace gravity_simulator_csharp
{
    internal class GlobularClusterStrategy : ISeedStrategy
    {
		Random random = new Random(1);

        public Body NextBody(Universe universe)
        {
			const uint dMin = 10;
			const uint dMax = 600;
			const float mMin = 1e12f;
			const float mMax = 2e12f;

			double tetha = random.NextDouble() * 2 * Math.PI;
			double distance = random.NextDouble() * (dMax - dMin) + dMin;
			double velocity = Math.Sqrt((universe.GravitationalConstant * mMax) / distance);
			float bodyMass = (float)random.NextDouble() * (mMax - mMin) + mMin;

            return new Body(
                mass: bodyMass,
                position: new Vector2((float)(Math.Cos(tetha) * distance), (float)(Math.Sin(tetha) * distance)),
                velocity: new Vector2((float)(Math.Cos(tetha - 0.5 * Math.PI) * velocity), (float)(Math.Sin(tetha - 0.5 * Math.PI) * velocity)),
                acceleration: Vector2.Zero
            );
        }
    }
}
