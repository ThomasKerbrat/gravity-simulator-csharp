using System.Numerics;

namespace gravity_simulator_csharp
{
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

		public override string ToString()
		{
			return $"{Origin.ToString()} {Width} {Height}";
		}
	}
}