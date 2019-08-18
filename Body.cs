using System;
using System.Numerics;

namespace gravity_simulator_csharp
{
	internal class Body
	{
		internal float Mass;
		internal Vector2 Position;
		internal Vector2 Velocity;
		internal Vector2 Acceleration;

		private float _radius;

		internal Body(float mass, Vector2 position, Vector2 velocity, Vector2 acceleration) 
		{
			this.Mass = mass;
			this.Position = position;
			this.Velocity = velocity;
			this.Acceleration = acceleration;

			ComputeRadius();
		}

		private void ComputeRadius()
		{
			_radius = (float)Math.Pow(3f / 4f * Mass / Math.PI, 1f / 3f) / 5e3f;
		}

		internal float Radius { get { return _radius; } }
	}
}
