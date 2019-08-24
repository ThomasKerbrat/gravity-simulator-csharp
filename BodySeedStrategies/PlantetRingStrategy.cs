using System;
using System.Numerics;

namespace gravity_simulator_csharp
{
    internal class PlanetRingStrategy : ISeedStrategy
    {
        bool didGenerateCentralBody = false;
		Random random = new Random(1);

        public Body NextBody(IUniverse universe)
        {
            const float centralBodyMass = 1e16f;

            if (didGenerateCentralBody == false)
            {
				didGenerateCentralBody = true;
                return new Body(
                    mass: centralBodyMass,
                    position: Vector2.Zero,
                    velocity: Vector2.Zero,
                    acceleration: Vector2.Zero
                );
            }

			const uint dMin = 300;
			const uint dMax = 500;
			const float mMin = centralBodyMass / 1e6f;
			const float mMax = centralBodyMass / 1e5f;

			double tetha = random.NextDouble() * 2 * Math.PI;
			double distance = random.NextDouble() * (dMax - dMin) + dMin;
			double velocity = Math.Sqrt((universe.GravitationalConstant * centralBodyMass) / distance);
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
