using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct FlyRenderer : ISharedComponentData, IEquatable<FlyRenderer>
{
    public Material material;

    public ShadowCastingMode castingMode;

    public bool receiveShadows;

    public bool Equals(FlyRenderer other)
    {
        return Equals(material, other.material) && castingMode == other.castingMode && receiveShadows == other.receiveShadows;
    }

    public override bool Equals(object obj)
    {
        return obj is FlyRenderer other && Equals(other);
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