using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace gravity_simulator_csharp
{
	public class QuadTree
	{
		private IBoundary Boundary;
		private uint LeafCapacity;
		private List<Vector2> Points;
		private QuadTree[] Nodes;

		public QuadTree(IBoundary boundary, uint leafCapacity)
		{
			this.Boundary = boundary;
			this.LeafCapacity = leafCapacity;
			Points = new List<Vector2>();
		}

		public bool Insert(Vector2 point)
		{
			if (Boundary.Contains(point) == false)
			{
				return false;
			}

			if (Points != null && Points.Count < LeafCapacity)
			{
				Points.Add(point);
				return true;
			}

			if (Nodes == null)
			{
				Subdivide();
			}

			foreach (QuadTree node in Nodes)
			{
				if (node.Insert(point))
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

			IBoundary[] boundaries = new Rectangle[]
			{
				new Rectangle(new Vector2(Boundary.Origin.X, Boundary.Origin.Y), width, height),
				new Rectangle(new Vector2((float)(Boundary.Origin.X + width), Boundary.Origin.Y), width, height),
				new Rectangle(new Vector2(Boundary.Origin.X, (float)(Boundary.Origin.Y + height)), width, height),
				new Rectangle(new Vector2((float)(Boundary.Origin.X + width), (float)(Boundary.Origin.Y + height)), width, height),
			};

			Nodes = new QuadTree[4];

			for (int i = 0; i < Nodes.Length; i++)
			{
				Nodes[i] = new QuadTree(boundaries[i], LeafCapacity);
			}

			foreach (Vector2 point in Points)
			{
				bool pointInserted = false;

				foreach (QuadTree tree in Nodes)
				{
					if (tree.Insert(point))
					{
						pointInserted = true;
						break;
					}
				}

				Debug.Assert(pointInserted == true, "Point were not inserted during subdivision.");
			}

			Points = null;
		}

		public List<Vector2> Query(IBoundary range)
		{
			var pointsInRange = new List<Vector2>();

			if (Boundary.Intersects(range) == false)
			{
				return pointsInRange;
			}

			if (Points != null)
			{
				foreach (Vector2 point in Points)
				{
					if (range.Contains(point))
					{
						pointsInRange.Add(point);
					}
				}
			}
			else
			{
				Debug.Assert(Nodes != null, "Nodes should not be null when Points are already null.");

				foreach (QuadTree tree in Nodes)
				{
					pointsInRange.AddRange(tree.Query(range));
				}
			}

			return pointsInRange;
		}
	}

	public interface IBoundary
	{
		Vector2 Origin { get; }
		double Width { get; }
		double Height { get; }

		bool Contains(Vector2 point);
		bool Intersects(IBoundary boundary);
	}

	public class Rectangle : IBoundary
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

		public bool Intersects(IBoundary boundary)
		{
			return !(boundary.Origin.X > Origin.X + Width
				|| boundary.Origin.Y > Origin.Y + Height
				|| boundary.Origin.X + boundary.Width < Origin.X
				|| boundary.Origin.Y + boundary.Height < Origin.Y);
		}
	}
}
