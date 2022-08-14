using Unity.Entities;

namespace FlyComponent
{
    [System.Serializable]
    public struct FlySpawn: ISharedComponentData, System.IEquatable<FlySpawn>
    {
        public UnityEngine.Mesh templateMesh;

        public bool Equals(FlySpawn other)
        {
            return Equals(templateMesh, other.templateMesh);
        }

        public override bool Equals(object obj)
        {
            return obj is FlySpawn other && Equals(other);
        }

        public override int GetHashCode()
        {
            return templateMesh != null ? templateMesh.GetHashCode() : 0;
        }
    }
}