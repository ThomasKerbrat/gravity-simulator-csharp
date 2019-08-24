using System;
using System.Collections.Generic;
using System.Numerics;

namespace gravity_simulator_csharp
{
	public class UniverseDirectSum : IUniverse
	{
		private const float _gravitationalConstant = 6.67408e-11f;
		private const uint _outwardBoundLimit = 2000;
		private readonly List<Body> _bodies;
		private readonly float _computationsPerSecond;

		public UniverseDirectSum(float computationsPerSecond, uint bodyNumber, ISeedStrategy seedStrategy)
		{
			ComputedTicks = 0;
			_computationsPerSecond = computationsPerSecond;
			_bodies = new List<Body>();

			for (int index = 0; index < bodyNumber; index++)
			{
				_bodies.Add(seedStrategy.NextBody(this));
			}
		}

		public UniverseDirectSum(float computationsPerSecond, List<Body> bodies)
		{
			_computationsPerSecond = computationsPerSecond;
			_bodies = bodies;
		}

		public float GravitationalConstant => _gravitationalConstant;
		public float OutwardBoundLimit => _outwardBoundLimit;

		public uint ComputedTicks { get; private set; }
		public float Duration => ComputedTicks / ComputationsPerSecond;
		public float ComputationsPerSecond => _computationsPerSecond;

		public IReadOnlyList<Body> Bodies => _bodies;
		public int BodyCount => _bodies.Count;

		public void Tick()
		{
			DeleteOutOfBoundBodies();

			List<Vector2> forces = ComputeForces();
			ShiftBodies(forces);
			CollideBodies();

			ComputedTicks++;
		}

		private void DeleteOutOfBoundBodies()
		{
			for (int index = _bodies.Count - 1; index >= 0; index--)
			{
				if (Vector2.Distance(Vector2.Zero, _bodies[index].Position) > OutwardBoundLimit)
				{
					_bodies.RemoveAt(index);
				}
			}
		}

		private List<Vector2> ComputeForces()
		{
			List<Vector2> forces = new List<Vector2>();

			foreach (Body body in _bodies)
			{
				forces.Add(Vector2.Zero);
			}

			for (int i = 0; i < _bodies.Count; i++)
			{
				for (int j = i + 1; j < _bodies.Count; j++)
				{
					Body a = _bodies[i];
					Body b = _bodies[j];
					float distance = Vector2.Distance(a.Position, b.Position);
					float force = _gravitationalConstant * ((a.Mass * b.Mass) / (float)Math.Pow(distance, 2));

					float angle = (float)Math.Atan2(b.Position.Y - a.Position.Y, b.Position.X - a.Position.X);
					forces[i] = new Vector2(
						forces[i].X + (float)Math.Cos(angle) * force,
						forces[i].Y + (float)Math.Sin(angle) * force
					);

					angle = (float)((angle + Math.PI) % (2 * Math.PI));
					forces[j] = new Vector2(
						forces[j].X + (float)Math.Cos(angle) * force,
						forces[j].Y + (float)Math.Sin(angle) * force
					);
				}
			}

			return forces;
		}

		private float ShiftBodies(List<Vector2> forces)
		{
			float maxRadius = float.MinValue;

			for (int index = 0; index < _bodies.Count; index++)
			{
				Body body = _bodies[index];

				body.Acceleration = new Vector2(
					forces[index].X / body.Mass,
					forces[index].Y / body.Mass
				);

				body.Velocity = new Vector2(
					body.Velocity.X + (body.Acceleration.X / ComputationsPerSecond),
					body.Velocity.Y + (body.Acceleration.Y / ComputationsPerSecond)
				);

				body.Position = new Vector2(
					body.Position.X + (body.Velocity.X / ComputationsPerSecond),
					body.Position.Y + (body.Velocity.Y / ComputationsPerSecond)
				);

				if (body.Radius > maxRadius) { maxRadius = body.Radius; }
			}

			return maxRadius;
		}

		private void CollideBodies()
		{
			for (int i = 0; i < _bodies.Count; i++)
			{
				for (int j = i + 1; j < _bodies.Count; j++)
				{
					Body a = _bodies[i];
					Body b = _bodies[j];

					if (a == null || b == null) { continue; }

					float distance = Vector2.Distance(a.Position, b.Position);

					if (distance < (a.Radius + b.Radius))
					{
						a.Position = new Vector2(
							(a.Position.X * a.Mass + b.Position.X * b.Mass) / (a.Mass + b.Mass),
							(a.Position.Y * a.Mass + b.Position.Y * b.Mass) / (a.Mass + b.Mass)
						);

						a.Velocity = new Vector2(
							(a.Velocity.X * a.Mass + b.Velocity.X * b.Mass) / (a.Mass + b.Mass),
							(a.Velocity.Y * a.Mass + b.Velocity.Y * b.Mass) / (a.Mass + b.Mass)
						);

						a.Acceleration = new Vector2(
							(a.Acceleration.X * a.Mass + b.Acceleration.X * b.Mass) / (a.Mass + b.Mass),
							(a.Acceleration.Y * a.Mass + b.Acceleration.Y * b.Mass) / (a.Mass + b.Mass)
						);

						a.Mass += b.Mass;
						_bodies[j] = null;
					}
				}
			}

			for (int index = _bodies.Count - 1; index >= 0; index--)
			{
				if (_bodies[index] == null)
				{
					_bodies.RemoveAt(index);
				}
			}
		}
	}
}
