using Butterfly.NativeItem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.FlyComponent
{
    [System.Serializable]
    public struct FlyRenderer: ISharedComponentData, System.IEquatable<FlyRenderer>
    {
        public const int kMaxVertices = 60000;
        public FlyRenderSettings settings;
        public NativeArray<float3> vertices;
        public NativeArray<float3> normals;
        public UnityEngine.Mesh meshInstance;
        public NativeCounter counter;

        public bool Equals(FlyRenderer other)
        {
            return settings.Equals(other.settings) &&
                   vertices.Equals(other.vertices) &&
                   normals.Equals(other.normals) &&
                   Equals(meshInstance, other.meshInstance) &&
                   counter.Equals(other.counter);
        }

        public override bool Equals(object obj)
        {
            return obj is FlyRenderer other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = settings.GetHashCode();
                hashCode = (hashCode * 397) ^ vertices.GetHashCode();
                hashCode = (hashCode * 397) ^ normals.GetHashCode();
                hashCode = (hashCode * 397) ^ (meshInstance != null ? meshInstance.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ counter.GetHashCode();
                return hashCode;
            }
        }
    }
}