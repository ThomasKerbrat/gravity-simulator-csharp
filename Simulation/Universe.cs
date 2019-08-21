using System;
using System.Collections.Generic;
using System.Numerics;

namespace gravity_simulator_csharp
{
	internal class Universe
	{
		private const float _gravitationalConstant = 6.67408e-11f;
		private const uint OutwardBoundLimit = 2000;
		private const float Theta = 0.25f;
		private uint ComputedTicks = 0;

		internal readonly uint ComputationsPerSecond;
		internal readonly List<Body> Bodies;

		internal Universe(uint computationsPerSecond, uint bodyNumber, ISeedStrategy seedStrategy)
		{
			this.ComputationsPerSecond = computationsPerSecond;
			this.Bodies = new List<Body>();

			for (int index = 0; index < bodyNumber; index++)
			{
				this.Bodies.Add(seedStrategy.NextBody(this));
			}
		}

		internal Universe(uint computationsPerSecond, List<Body> bodies)
		{
			this.ComputationsPerSecond = computationsPerSecond;
			this.Bodies = bodies;
		}

		internal float Duration
		{
			get { return ComputedTicks / (float)ComputationsPerSecond; }
		}

		internal float GravitationalConstant
		{
			get { return _gravitationalConstant; }
		}

		internal void Tick()
		{
			DeleteOutOfBoundBodies();

			// List<Vector2> forces1 = ComputeForcesBruteHalf();
			// ShiftBodies(forces1);
			// CollideBodiesBruteHalf();

			BarnesHutTree tree;
			tree = ComputeBarnesHutTree();
			List<Vector2> forces2 = ComputeForcesBarnesHut(tree);
			float maxRadius = ShiftBodies(forces2);
			tree = ComputeBarnesHutTree();
			CollideBodiesHarnesHutTree(tree, maxRadius);

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

		private BarnesHutTree ComputeBarnesHutTree()
		{
			float minX = float.MaxValue;
			float maxX = float.MinValue;
			float minY = float.MaxValue;
			float maxY = float.MinValue;

			foreach (Body body in Bodies)
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

			foreach (Body body in Bodies)
			{
				tree.Insert(body);
			}

			return tree;
		}

		private List<Vector2> ComputeForcesBruteHalf()
		{
			List<Vector2> forces = new List<Vector2>();

			foreach (Body body in Bodies)
			{
				forces.Add(Vector2.Zero);
			}

			for (int i = 0; i < Bodies.Count; i++)
			{
				for (int j = i + 1; j < Bodies.Count; j++)
				{
					Body a = Bodies[i];
					Body b = Bodies[j];
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

		private List<Vector2> ComputeForcesBarnesHut(BarnesHutTree tree)
		{
			var forces = new List<Vector2>();

			foreach (Body a in Bodies)
			{
				var forcesOnBody = Vector2.Zero;
				List<VirtualBody> virtualBodies = tree.Query(a, Theta);

				foreach (VirtualBody b in virtualBodies)
				{
					float distance = Vector2.Distance(a.Position, b.Position);
					float force = GravitationalConstant * ((a.Mass * b.Mass) / (float)Math.Pow(distance, 2));

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

			for (int index = 0; index < Bodies.Count; index++)
			{
				Body body = Bodies[index];

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

		private void CollideBodiesBruteHalf()
		{
			for (int i = 0; i < Bodies.Count; i++)
			{
				for (int j = i + 1; j < Bodies.Count; j++)
				{
					Body a = Bodies[i];
					Body b = Bodies[j];

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
						Bodies[j] = null;
					}
				}
			}

			for (int index = Bodies.Count - 1; index >= 0; index--)
			{
				if (Bodies[index] == null)
				{
					Bodies.RemoveAt(index);
				}
			}
		}

		private void CollideBodiesHarnesHutTree(BarnesHutTree tree, float maxRadius)
		{
			var collisions = new Dictionary<Body, HashSet<Body>>();

			foreach (Body a in Bodies)
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
					Bodies.Remove(body);
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
