using System;
using System.Collections.Generic;
using System.Numerics;

namespace gravity_simulator_csharp
{
	internal class Universe
	{
		private const float GravitationalConstant = 6.67408e-11f;
		private const uint OutwardBoundLimit = 2000;
		private uint ComputedTicks = 0;

		internal readonly uint ComputationsPerSecond;
		internal readonly List<Body> Bodies;

		internal Universe(uint computationsPerSecond, List<Body> bodies)
		{
			this.ComputationsPerSecond = computationsPerSecond;
			this.Bodies = bodies;
		}

		internal float Duration
		{
			get { return ComputedTicks / (float)ComputationsPerSecond; }
		}

		internal void Tick()
		{
			DeleteOutOfBoundBodies();
			List<Vector2> forces = ComputeForce();
			ShiftBodies(forces);
			ComputedTicks++;
		}

		private void DeleteOutOfBoundBodies()
		{
			for (int index = Bodies.Count - 1; index >= 0; index--)
			{
				if (Vector2.Distance(Vector2.Zero, Bodies[index].Position) > OutwardBoundLimit)
				{
					Bodies.RemoveAt(index);
				}
			}
		}

		private List<Vector2> ComputeForce()
		{
			List<Vector2> forces = new List<Vector2>();

			foreach (Body bodyA in Bodies)
			{
				Vector2 acceleration = Vector2.Zero;

				foreach (Body bodyB in Bodies)
				{
					if (bodyB != bodyA)
					{
						float distance = Vector2.Distance(bodyA.Position, bodyB.Position);
						float force = GravitationalConstant * ((bodyA.Mass * bodyB.Mass) / (float)Math.Pow(distance, 2));

						float angle = (float)Math.Atan2(bodyB.Position.Y - bodyA.Position.Y, bodyB.Position.X - bodyA.Position.X);
						acceleration.X += (float)Math.Cos(angle) * force;
						acceleration.Y += (float)Math.Sin(angle) * force;
					}
				}

				forces.Add(acceleration);
			}

			return forces;
		}

		private void ShiftBodies(List<Vector2> forces)
		{
			for (int index = 0; index < Bodies.Count; index++)
			{
				Body body = Bodies[index];

				body.Acceleration.X = forces[index].X / body.Mass;
				body.Acceleration.Y = forces[index].Y / body.Mass;

				body.Velocity.X += body.Acceleration.X / ComputationsPerSecond;
				body.Velocity.Y += body.Acceleration.Y / ComputationsPerSecond;

				body.Position.X += body.Velocity.X / ComputationsPerSecond;
				body.Position.Y += body.Velocity.Y / ComputationsPerSecond;
			}
		}
	}
}
