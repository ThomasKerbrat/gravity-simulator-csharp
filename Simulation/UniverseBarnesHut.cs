using System;
using System.Collections.Generic;
using System.Numerics;

namespace gravity_simulator_csharp
{
	public class UniverseBarnesHut : IUniverse
	{
		private const float _gravitationalConstant = 6.67408e-11f;
		private const uint _outwardBoundLimit = 10_000;
		private const float Theta = 0.25f;
		private readonly List<Body> _bodies;
		private readonly float _computationsPerSecond;

		public UniverseBarnesHut(float computationsPerSecond, uint bodyNumber, ISeedStrategy seedStrategy)
		{
			ComputedTicks = 0;
			_computationsPerSecond = computationsPerSecond;
			_bodies = new List<Body>();

			for (int index = 0; index < bodyNumber; index++)
			{
				_bodies.Add(seedStrategy.NextBody(this));
			}
		}

		public UniverseBarnesHut(float computationsPerSecond, List<Body> bodies)
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

			BarnesHutTree tree;
			tree = ComputeBarnesHutTree();
			List<Vector2> forces = ComputeForces(tree);
			float maxRadius = ShiftBodies(forces);
			tree = ComputeBarnesHutTree();
			CollideBodies(tree, maxRadius);

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

		private BarnesHutTree ComputeBarnesHutTree()
		{
			float minX = float.MaxValue;
			float maxX = float.MinValue;
			float minY = float.MaxValue;
			float maxY = float.MinValue;

			foreach (Body body in _bodies)
			{
				if (body.Position.X < minX) { minX = body.Position.X; }
				if (body.Position.X > maxX) { maxX = body.Position.X; }
				if (body.Position.Y < minY) { minY = body.Position.Y; }
				if (body.Position.Y > maxY) { maxY = body.Position.Y; }
			}

			float width = Math.Abs(maxX - minX);
			float height = Math.Abs(maxY - minY);
			float size = (float)Math.Ceiling(Math.Max(width, height) + 0.5);
			var middle = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
			var origin = new Vector2(middle.X - size / 2, middle.Y - size / 2);

			var tree = new BarnesHutTree(new Rectangle(origin, size, size), 4);

			foreach (Body body in _bodies)
			{
				tree.Insert(body);
			}

			return tree;
		}

		private List<Vector2> ComputeForces(BarnesHutTree tree)
		{
			var forces = new List<Vector2>();

			foreach (Body a in _bodies)
			{
				var forcesOnBody = Vector2.Zero;
				List<VirtualBody> virtualBodies = tree.Query(a, Theta);

				foreach (VirtualBody b in virtualBodies)
				{
					float distance = Vector2.Distance(a.Position, b.Position);
					float force = GravitationalConstant * ((a.Mass * b.Mass) / (float)Math.Pow(distance, 2));

					// if (distance < float.Epsilon) 
					// {
					// 	Console.WriteLine("d {0} f {1} a {2} b {3}", distance, force, a, b);
					// 	throw new Exception("Epsylon");
					// }

					// if (float.IsNaN(force))
					// {
					// 	Console.WriteLine("d {0} f {1} a {2} b {3}", distance, force, a, b);
					// 	throw new Exception("NaN");
					// }

					float angle = (float)Math.Atan2(b.Position.Y - a.Position.Y, b.Position.X - a.Position.X);
					forcesOnBody.X += (float)Math.Cos(angle) * force;
					forcesOnBody.Y += (float)Math.Sin(angle) * force;
				}

				forces.Add(forcesOnBody);
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

				// if (float.IsNaN(body.Position.X)) {
				// 	Console.WriteLine("f {0} p {1} v {2} a {3}", forces[index], body.Position, body.Velocity, body.Acceleration);
				// 	throw new Exception("NaN");
				// }

				if (body.Radius > maxRadius) { maxRadius = body.Radius; }
			}

			return maxRadius;
		}

		private void CollideBodies(BarnesHutTree tree, float maxRadius)
		{
			var collisions = new Dictionary<Body, HashSet<Body>>();

			foreach (Body a in _bodies)
			{
				Vector2 origin = new Vector2(a.Position.X - 2 * maxRadius, a.Position.Y - 2 * maxRadius);
				Rectangle range = new Rectangle(origin, 4 * maxRadius, 4 * maxRadius);
				List<Body> closeBodies = tree.Query(range);

				foreach (Body b in closeBodies)
				{
					if (a == b) { continue; }

					float distance = Vector2.Distance(a.Position, b.Position);

					if (distance < (a.Radius + b.Radius))
					{
						// If there is A, there must not be B inside of it.
						if (collisions.ContainsKey(a) && collisions.GetValueOrDefault(a).Contains(b) == false)
						{
							collisions.GetValueOrDefault(a).Add(b);
						}
						// Either there is no B, or if there is, there must not be A inside of B.
						else if (collisions.ContainsKey(b) == false || (collisions.ContainsKey(b) && collisions.GetValueOrDefault(b).Contains(a) == false))
						{
							collisions.Add(a, new HashSet<Body>() { b });
						}
					}
				}
			}

			foreach (var collision in collisions)
			{
				ComputeCollision(collision.Key, collision.Value);

				foreach (Body body in collision.Value)
				{
					_bodies.Remove(body);
				}
			}
		}

		private void ComputeCollision(Body a, IEnumerable<Body> bodies)
		{
			float totalMass = a.Mass;
			float posX = a.Position.X * a.Mass;
			float posY = a.Position.Y * a.Mass;
			float velX = a.Velocity.X * a.Mass;
			float velY = a.Velocity.Y * a.Mass;
			float accX = a.Acceleration.X * a.Mass;
			float accY = a.Acceleration.Y * a.Mass;

			foreach (Body b in bodies)
			{
				totalMass += b.Mass;
				posX += b.Position.X * b.Mass;
				posY += b.Position.Y * b.Mass;
				velX += b.Velocity.X * b.Mass;
				velY += b.Velocity.Y * b.Mass;
				accX += b.Acceleration.X * b.Mass;
				accY += b.Acceleration.Y * b.Mass;
			}

			a.Mass = totalMass;
			a.Position = new Vector2(posX / totalMass, posY / totalMass);
			a.Velocity = new Vector2(velX / totalMass, velY / totalMass);
			a.Acceleration = new Vector2(accX / totalMass, accY / totalMass);
		}
	}
}
