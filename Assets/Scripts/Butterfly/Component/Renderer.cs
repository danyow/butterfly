using Butterfly.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.Component
{
    [System.Serializable]
    public struct Renderer: ISharedComponentData, System.IEquatable<Renderer>
    {
        public const int kMaxVertices = 510000;
        public RenderSettings settings;
        public NativeArray<float3> vertices;
        public NativeArray<float3> normals;
        public UnityEngine.Mesh workMesh;
        public NativeCounter counter;

        public bool Equals(Renderer other)
        {
            return settings.Equals(other.settings) &&
                   vertices.Equals(other.vertices) &&
                   normals.Equals(other.normals) &&
                   Equals(workMesh, other.workMesh) &&
                   counter.Equals(other.counter);
        }

        public override bool Equals(object obj)
        {
            return obj is Renderer other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = settings.GetHashCode();
                hashCode = (hashCode * 397) ^ vertices.GetHashCode();
                hashCode = (hashCode * 397) ^ normals.GetHashCode();
                hashCode = (hashCode * 397) ^ (workMesh != null ? workMesh.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ counter.GetHashCode();
                return hashCode;
            }
        }
    }
}