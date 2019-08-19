using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace gravity_simulator_csharp
{
	public struct VirtualBody
	{
		public Vector2 Position;
		public float Mass;

		public static VirtualBody FromBody(Body body)
		{
			return new VirtualBody() { Mass = body.Mass, Position = body.Position };
		}
	}
}
