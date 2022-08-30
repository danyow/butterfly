using Unity.Entities;

namespace Butterfly.Component
{
    [System.Serializable]
    internal struct RenderSettings: ISharedComponentData, System.IEquatable<RenderSettings>
    {
        public UnityEngine.Material material;
        public UnityEngine.Rendering.ShadowCastingMode castShadows;
        public bool receiveShadows;

        public bool Equals(RenderSettings other)
        {
            return Equals(material, other.material) && castShadows == other.castShadows && receiveShadows == other.receiveShadows;
        }

        public override bool Equals(object obj)
        {
            return obj is RenderSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (material != null ? material.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)castShadows;
                hashCode = (hashCode * 397) ^ receiveShadows.GetHashCode();
                return hashCode;
            }
        }
    }
}