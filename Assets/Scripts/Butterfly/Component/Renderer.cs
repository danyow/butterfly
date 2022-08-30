using Butterfly.Utility;
using Unity.Entities;

namespace Butterfly.Component
{
    [System.Serializable]
    internal struct Renderer: ISharedComponentData, System.IEquatable<Renderer>
    {
        public const int MaxVertices = 510000;
        public RenderSettings settings;
        public UnityEngine.Vector3[] vertices;
        public UnityEngine.Vector3[] normals;
        public UnityEngine.Mesh workMesh;
        public NativeCounter counter;
        public NativeCounter.Concurrent concurrentCounter;

        public bool Equals(Renderer other)
        {
            return settings.Equals(other.settings) &&
                   Equals(vertices, other.vertices) &&
                   Equals(normals, other.normals) &&
                   Equals(workMesh, other.workMesh) &&
                   counter.Equals(other.counter) &&
                   concurrentCounter.Equals(other.concurrentCounter);
        }

        public override bool Equals(object obj)
        {
            return obj is Renderer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(settings, vertices, normals, workMesh, counter, concurrentCounter);
        }
    }
}