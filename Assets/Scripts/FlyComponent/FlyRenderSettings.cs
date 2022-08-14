using Unity.Entities;

namespace FlyComponent
{
    [System.Serializable]
    public struct FlyRenderSettings: ISharedComponentData, System.IEquatable<FlyRenderSettings>
    {
        public UnityEngine.Material material;
        public UnityEngine.Rendering.ShadowCastingMode castingMode;
        public bool receiveShadows;

        public bool Equals(FlyRenderSettings other)
        {
            return Equals(material, other.material) && castingMode == other.castingMode && receiveShadows == other.receiveShadows;
        }

        public override bool Equals(object obj)
        {
            return obj is FlyRenderSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (material != null ? material.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)castingMode;
                hashCode = (hashCode * 397) ^ receiveShadows.GetHashCode();
                return hashCode;
            }
        }
    }
}