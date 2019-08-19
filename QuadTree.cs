using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace gravity_simulator_csharp
{
	public class BarnesHutTree
	{
		private Rectangle Boundary;
		private uint LeafCapacity;
		private List<Body> Bodies;
		private BarnesHutTree[] Nodes;

		public BarnesHutTree(Rectangle boundary, uint leafCapacity)
		{
			this.Boundary = boundary;
			this.LeafCapacity = leafCapacity;
			Bodies = new List<Body>();
		}

		private float GetTotalMass(Body exclude)
		{
			float _totalMass = 0;

			if (Bodies != null)
			{
				foreach (Body body in Bodies)
				{
					if (body != exclude)
					{
						_totalMass += body.Mass;
					}
				}
			}
			else if (Nodes != null)
			{
				foreach (BarnesHutTree tree in Nodes)
				{
					_totalMass += tree.GetTotalMass(exclude);
				}
			}

			return _totalMass;
		}

		private Nullable<Vector2> GetCenterOfMass(Body exclude)
		{
			if (Bodies != null && Bodies.Count == 0 && Nodes == null)
			{
				return null;
			}

			var bodies = new List<Body>();

			if (Bodies != null)
			{
				foreach (Body body in Bodies)
				{
					if (body != exclude)
					{
						bodies.AddRange(Bodies);
					}
				}
			}
			else if (Nodes != null)
			{
				foreach (BarnesHutTree tree in Nodes)
				{
					if (tree.GetCenterOfMass(exclude).HasValue == true)
					{
						bodies.Add(new Body(tree.GetTotalMass(exclude), tree.GetCenterOfMass(exclude).Value, Vector2.Zero, Vector2.Zero));
					}
				}
			}

			if (bodies.Count == 0)
			{
				return null;
			}

			float totalMass = 0;
			float allX = 0;
			float allY = 0;

			foreach (Body body in bodies)
			{
				totalMass += body.Mass;
				allX += body.Position.X * body.Mass;
				allY += body.Position.Y * body.Mass;
			}

			return new Vector2(allX / totalMass, allY / totalMass);
		}

		public bool Insert(Body body)
		{
			if (Boundary.Contains(body) == false)
			{
				return false;
			}

			if (Bodies != null && Bodies.Count < LeafCapacity)
			{
				Bodies.Add(body);
				return true;
			}

			if (Nodes == null)
			{
				Subdivide();
			}

			foreach (BarnesHutTree node in Nodes)
			{
				if (node.Insert(body))
				{
					return true;
				}
			}

			return false;
		}

		private void Subdivide()
		{
			double width = Boundary.Width / 2;
			double height = Boundary.Height / 2;

			Rectangle[] boundaries = new Rectangle[]
			{
				new Rectangle(new Vector2(Boundary.Origin.X, Boundary.Origin.Y), width, height),
				new Rectangle(new Vector2((float)(Boundary.Origin.X + width), Boundary.Origin.Y), width, height),
				new Rectangle(new Vector2(Boundary.Origin.X, (float)(Boundary.Origin.Y + height)), width, height),
				new Rectangle(new Vector2((float)(Boundary.Origin.X + width), (float)(Boundary.Origin.Y + height)), width, height),
			};

			Nodes = new BarnesHutTree[4];

			for (int i = 0; i < Nodes.Length; i++)
			{
				Nodes[i] = new BarnesHutTree(boundaries[i], LeafCapacity);
			}

			foreach (Body body in Bodies)
			{
				bool pointInserted = false;

				foreach (BarnesHutTree tree in Nodes)
				{
					if (tree.Insert(body))
					{
						pointInserted = true;
						break;
					}
				}

				Debug.Assert(pointInserted == true, "Point were not inserted during subdivision.");
			}

			Bodies = null;
		}

		public List<Body> Query(Rectangle range)
		{
			var bodiesInRange = new List<Body>();

			if (Boundary.Intersects(range) == false)
			{
				return bodiesInRange;
			}

			if (Bodies != null)
			{
				foreach (Body body in Bodies)
				{
					if (range.Contains(body))
					{
						bodiesInRange.Add(body);
					}
				}
			}
			else
			{
				Debug.Assert(Nodes != null, "Nodes should not be null when Points are already null.");

				foreach (BarnesHutTree tree in Nodes)
				{
					bodiesInRange.AddRange(tree.Query(range));
				}
			}

			return bodiesInRange;
		}

		public List<Body> Query(Body body, float theta)
		{
			var virtualBodies = new List<Body>();
			Nullable<Vector2> centerOfMass;

			centerOfMass = GetCenterOfMass(body);

			if (Bodies != null && Bodies.Count > 0 && centerOfMass.HasValue)
			{
				virtualBodies.Add(new Body(
					GetTotalMass(body),
					centerOfMass.Value,
					Vector2.Zero,
					Vector2.Zero
				));
			}
			else if (Nodes != null)
			{
				foreach (BarnesHutTree tree in Nodes)
				{
					centerOfMass = tree.GetCenterOfMass(body);

					if (centerOfMass.HasValue == true)
					{
						float distance = Vector2.Distance(centerOfMass.Value, body.Position);
						float localTheta = (float)(tree.Boundary.Width / distance);

						if (localTheta >= theta)
						{
							virtualBodies.AddRange(tree.Query(body, theta));
						}
						else
						{
							virtualBodies.Add(
								new Body(tree.GetTotalMass(body),
								centerOfMass.Value,
								Vector2.Zero,
								Vector2.Zero
							));
						}
					}
				}
			}

			return virtualBodies;
		}
	}

	public class Rectangle
	{
		public Rectangle(Vector2 Origin, double Width, double Height)
		{
			this.Origin = Origin;
			this.Width = Width;
			this.Height = Height;
		}

		public Vector2 Origin { get; private set; }
		public double Width { get; private set; }
		public double Height { get; private set; }

		public bool Contains(Body body)
		{
			return body.Position.X >= Origin.X && body.Position.X <= Origin.X + Width
				&& body.Position.Y >= Origin.Y && body.Position.Y <= Origin.Y + Height;
		}

		public bool Intersects(Rectangle boundary)
		{
			return !(boundary.Origin.X > Origin.X + Width
				|| boundary.Origin.Y > Origin.Y + Height
				|| boundary.Origin.X + boundary.Width < Origin.X
				|| boundary.Origin.Y + boundary.Height < Origin.Y);
		}
	}
}
