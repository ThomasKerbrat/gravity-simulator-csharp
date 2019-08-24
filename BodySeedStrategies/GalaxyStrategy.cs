using System;
using System.Collections.Generic;
using System.Numerics;

namespace gravity_simulator_csharp
{
	public class GalaxyStrategy
	{
		private static float gravitationalConstant = 6.67408e-11f;
		private static Random random = new Random(1);

		public static List<Body> GetBodies(uint bodyCount)
		{
			var bodies = new List<Body>();
			const double centralBodyMass = 1e16;

			bodies.AddRange(GalaxyStrategy.GetGalaxy((uint)Math.Truncate(bodyCount * 0.75), centralBodyMass, 0, 1000, 2000));
			bodies.AddRange(GalaxyStrategy.GetGalaxy((uint)Math.Truncate(bodyCount * 0.25), centralBodyMass, Math.PI, 4000, 500));

			return bodies;
		}

		private static List<Body> GetGalaxy(uint bodyCount, double centralBodyMass, double centralTetha, double centralDistance, double width)
		{
			var bodies = new List<Body>();

			double centralVelocity = 1;
			Vector2 galaxyOrigin;
			Vector2 galaxyVelocity;

			galaxyOrigin = new Vector2
			(
				(float)(Math.Cos(centralTetha) * centralDistance),
				(float)(Math.Cos(centralTetha) * centralDistance)
			);

			galaxyVelocity = new Vector2
			(
				(float)(Math.Cos(centralTetha - 0.5 * Math.PI) * centralVelocity),
				(float)(Math.Sin(centralTetha - 0.5 * Math.PI) * centralVelocity)
			);

			bodies.Add(new Body(
				mass: (float)centralBodyMass,
				position: galaxyOrigin,
				velocity: galaxyVelocity,
				acceleration: Vector2.Zero
			));

			const double dMin = 10;
			double dMax = width;
			const float mMin = 1e09f;
			const float mMax = 1e10f;

			for (int i = 0; i < bodyCount - 1; i++)
			{
				double tetha = random.NextDouble() * 2 * Math.PI;
				double distance = random.NextDouble() * (dMax - dMin) + dMin;
				double velocity = Math.Sqrt((gravitationalConstant * centralBodyMass) / distance);
				float bodyMass = (float)random.NextDouble() * (mMax - mMin) + mMin;

				var bodyPosition = new Vector2((float)(Math.Cos(tetha) * distance), (float)(Math.Sin(tetha) * distance));
				var bodyVelocity = new Vector2((float)(Math.Cos(tetha - 0.5 * Math.PI) * velocity), (float)(Math.Sin(tetha - 0.5 * Math.PI) * velocity));

				bodies.Add(new Body(
					mass: bodyMass,
					position: Vector2.Add(galaxyOrigin, bodyPosition),
					velocity: Vector2.Add(galaxyVelocity, bodyVelocity),
					acceleration: Vector2.Zero
				));
			}

			return bodies;
		}
	}
}
