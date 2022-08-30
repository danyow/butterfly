using Unity.Entities;

namespace Butterfly.Component
{
    [System.Serializable]
    internal struct Instance: ISharedComponentData, System.IEquatable<Instance>
    {
        public UnityEngine.Mesh templateMesh;

        public bool Equals(Instance other)
        {
            return Equals(templateMesh, other.templateMesh);
        }

        public override bool Equals(object obj)
        {
            return obj is Instance other && Equals(other);
        }

        public override int GetHashCode()
        {
            return templateMesh != null ? templateMesh.GetHashCode() : 0;
        }
    }
}