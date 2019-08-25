using System;
using System.IO;
using System.Collections.Generic;

namespace gravity_simulator_csharp
{
	internal class SerializedUniverse
	{
		float framesPerSecond;

		List<List<float[]>> framesBodiesCoordinates;

		internal SerializedUniverse(int bodyNumber, float seconds, float framesPerSecond)
		{
			this.framesPerSecond = framesPerSecond;
			framesBodiesCoordinates = new List<List<float[]>>();
		}

		internal void SnapshotUniverse(IUniverse universe)
		{
			List<float[]> bodies = new List<float[]>();

			foreach (Body body in universe.Bodies)
			{
				bodies.Add(new float[] { body.Radius, body.Position.X, body.Position.Y });
			}

			framesBodiesCoordinates.Add(bodies);
		}

		internal byte[] Serialize()
		{
			var stream = new MemoryStream();
			stream.Write(BitConverter.GetBytes((uint)1)); // version
			stream.Write(BitConverter.GetBytes(framesPerSecond)); // frames per second

			foreach (List<float[]> frame in framesBodiesCoordinates)
			{
				stream.Write(BitConverter.GetBytes((uint)frame.Count));

				foreach (float[] values in frame)
				{
					stream.Write(BitConverter.GetBytes(values[0]));
					stream.Write(BitConverter.GetBytes(values[1]));
					stream.Write(BitConverter.GetBytes(values[2]));
				}
			}

			return stream.GetBuffer();
		}
	}
}
