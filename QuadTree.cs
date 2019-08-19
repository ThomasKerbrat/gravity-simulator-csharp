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

		private bool _didComputeTotalMass = false;
		private float _totalMass;
		private bool _didComputeCenterOfMass = false;
		private Vector2? _centerOfMass;

		public BarnesHutTree(Rectangle boundary, uint leafCapacity)
		{
			this.Boundary = boundary;
			this.LeafCapacity = leafCapacity;
			Bodies = new List<Body>();
		}

		private float GetTotalMass(Body exclude)
		{
			if (_didComputeTotalMass == true && Boundary.Contains(exclude.Position) == false)
			{
				return _totalMass;
			}

			float totalMass = 0;

			if (Bodies != null)
			{
				foreach (Body body in Bodies)
				{
					if (body != exclude)
					{
						totalMass += body.Mass;
					}
				}
			}
			else if (Nodes != null)
			{
				foreach (BarnesHutTree tree in Nodes)
				{
					totalMass += tree.GetTotalMass(exclude);
				}
			}

			if (Boundary.Contains(exclude.Position) == false)
			{
				_totalMass = totalMass;
				_didComputeTotalMass = true;
			}

			return _totalMass;
		}

		private Vector2? GetCenterOfMass(Body exclude)
		{
			// Checking if there is no bodies or no sub-trees.
			if (Bodies != null && Bodies.Count == 0 && Nodes == null)
			{
				return null;
			}

			// Checking if we can use the cached value (if it has already been computed).
			if (_didComputeCenterOfMass == true && Boundary.Contains(exclude.Position) == false)
			{
				return _centerOfMass;
			}

			// Getting the bodies from which to compute the center of mass.
			var bodies = new List<VirtualBody>();

			if (Bodies != null)
			{
				foreach (Body body in Bodies)
				{
					if (body != exclude)
					{
						bodies.Add(VirtualBody.FromBody(body));
					}
				}
			}
			else if (Nodes != null)
			{
				Vector2? nodesCenterOfMass;
				VirtualBody newBody;
				float treesTotalMass;

				foreach (BarnesHutTree tree in Nodes)
				{
					nodesCenterOfMass = tree.GetCenterOfMass(exclude);
					if (nodesCenterOfMass.HasValue == true)
					{
						treesTotalMass = tree.GetTotalMass(exclude);
						newBody = new VirtualBody()
						{
							Mass = treesTotalMass,
							Position = nodesCenterOfMass.Value,
						};
						bodies.Add(newBody);
					}
				}
			}

			if (bodies.Count == 0)
			{
				return null;
			}

			// Actually computing the center of mass.
			float totalMass = 0;
			float allX = 0;
			float allY = 0;

			foreach (VirtualBody body in bodies)
			{
				totalMass += body.Mass;
				allX += body.Position.X * body.Mass;
				allY += body.Position.Y * body.Mass;
			}

			var centerOfMass = new Vector2(allX / totalMass, allY / totalMass);

			// Caching for later use (only if there is no body to exclude).
			if (Boundary.Contains(exclude.Position) == false)
			{
				_didComputeCenterOfMass = true;
				_centerOfMass = centerOfMass;
			}

			return centerOfMass;
		}

		public bool Insert(Body body)
		{
			if (Boundary.Contains(body.Position) == false)
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
					if (range.Contains(body.Position))
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

		public List<VirtualBody> Query(Body body, float theta)
		{
			var virtualBodies = new List<VirtualBody>();

			Vector2? centerOfMass;
			centerOfMass = GetCenterOfMass(body);

			if (Bodies != null && Bodies.Count > 0 && centerOfMass.HasValue)
			{
				virtualBodies.Add(new VirtualBody()
				{
					Mass = GetTotalMass(body),
					Position = centerOfMass.Value,
				});
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
							virtualBodies.Add(new VirtualBody()
							{
								Mass = tree.GetTotalMass(body),
								Position = centerOfMass.Value,
							});
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

		public bool Contains(Vector2 point)
		{
			return point.X >= Origin.X && point.X <= Origin.X + Width
				&& point.Y >= Origin.Y && point.Y <= Origin.Y + Height;
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
