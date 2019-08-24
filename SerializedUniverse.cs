using System.Collections.Generic;
using System.Runtime.Serialization;

namespace gravity_simulator_csharp
{
    [DataContract]
    internal class SerializedUniverse
    {
		[DataMember]
		float framesPerSecond;

        [DataMember]
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
    }
}
