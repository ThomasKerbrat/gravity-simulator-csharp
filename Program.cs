using System;
using System.Collections.Generic;
using System.Numerics;

namespace gravity_simulator_csharp
{
	class Program
	{
		static void Main(string[] args)
		{
			List<Body> bodies = new List<Body>();
			bodies.Add(new Body(mass: 1e12F, position: new Vector2(-5, -5), velocity: Vector2.Zero, acceleration: Vector2.Zero));
			bodies.Add(new Body(mass: 1e12F, position: new Vector2(-5, 5), velocity: Vector2.Zero, acceleration: Vector2.Zero));
			bodies.Add(new Body(mass: 1e12F, position: new Vector2(5, 0), velocity: Vector2.Zero, acceleration: Vector2.Zero));

			Universe universe = new Universe(computationsPerSecond: 100, bodies);

			const uint framesPerSecond = 5;
			const float durationBetweenFrames = 1f / framesPerSecond;
			float durationSinceLastSnapshot = 0;

			while (universe.Duration < 5)
			{
				universe.Tick();
				durationSinceLastSnapshot += 1f / universe.ComputationsPerSecond;

				if (durationSinceLastSnapshot > durationBetweenFrames)
				{
					durationSinceLastSnapshot = 0;

					foreach (Body body in universe.Bodies)
					{
						Console.WriteLine("X: {0}, Y: {1}", body.Position.X, body.Position.Y);
					}

					Console.WriteLine("");
				}
			}
		}
	}
}
