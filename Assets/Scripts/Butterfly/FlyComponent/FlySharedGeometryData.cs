using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.FlyComponent
{
    public struct FlySharedGeometryData: ISharedComponentData, IEquatable<FlySharedGeometryData>
    {
        public const int kMaxVertices = 60000;
        public NativeArray<float3> vertices;
        public NativeArray<float3> normals;
        public UnityEngine.Mesh meshInstance;

        public bool Equals(FlySharedGeometryData other)
        {
            return vertices.Equals(other.vertices) && normals.Equals(other.normals) && Equals(meshInstance, other.meshInstance);
        }

        public override bool Equals(object obj)
        {
            return obj is FlySharedGeometryData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = vertices.GetHashCode();
                hashCode = (hashCode * 397) ^ normals.GetHashCode();
                hashCode = (hashCode * 397) ^ (meshInstance != null ? meshInstance.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}