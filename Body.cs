using System.Numerics;

namespace gravity_simulator_csharp
{
	internal class Body
	{
		internal float Mass;
		internal Vector2 Position;
		internal Vector2 Velocity;
		internal Vector2 Acceleration;

		internal Body(float mass, Vector2 position, Vector2 velocity, Vector2 acceleration) 
		{
			this.Mass = mass;
			this.Position = position;
			this.Velocity = velocity;
			this.Acceleration = acceleration;
		}
	}
}
