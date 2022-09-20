using Unity.Entities;

namespace Butterfly.Component
{
    [System.Serializable]
    internal struct Instance: ISharedComponentData, System.IEquatable<Instance>
    {
        /// <summary>
        /// 模板网格
        /// </summary>
        public UnityEngine.Mesh templateMesh;

        /// <summary>
        /// 特效率
        /// </summary>
        public float effectRate;

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