using System.Collections.Generic;
using System.Runtime.Serialization;

namespace gravity_simulator_csharp
{
    [DataContract]
    internal class SerializedUniverse
    {
		[DataMember]
		uint framesPerSecond;

        [DataMember]
        List<List<float[]>> framesBodiesCoordinates;

        internal SerializedUniverse(uint bodyNumber, uint seconds, uint framesPerSecond)
        {
			this.framesPerSecond = framesPerSecond;
            framesBodiesCoordinates = new List<List<float[]>>();
        }

        internal void SnapshotUniverse(Universe universe)
        {
            List<float[]> bodies = new List<float[]>();

            foreach (Body body in universe.Bodies)
            {
                bodies.Add(new float[] { body.Radius, body.Position.X, body.Position.Y });
            }

            framesBodiesCoordinates.Add(bodies);
        }
    }
}
