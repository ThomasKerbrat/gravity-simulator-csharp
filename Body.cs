using System;
using System.Numerics;

namespace gravity_simulator_csharp
{
	public class Body
	{
		private float _radius;
		private float _mass;

		public Body(float mass, Vector2 position, Vector2 velocity, Vector2 acceleration)
		{
			this.Mass = mass;
			this.Position = position;
			this.Velocity = velocity;
			this.Acceleration = acceleration;

			ComputeRadius();
		}

		public float Mass
		{
			get
			{
				return _mass;
			}

			internal set
			{
				_mass = value;
				ComputeRadius();
			}
		}

		public Vector2 Position { get; internal set; }
		public Vector2 Velocity { get; internal set; }
		public Vector2 Acceleration { get; internal set; }

		private void ComputeRadius()
		{
			_radius = (float)Math.Pow(3f / 4f * Mass / Math.PI, 1f / 3f) / 5e3f;
		}

		public float Radius { get { return _radius; } }
	}
}
