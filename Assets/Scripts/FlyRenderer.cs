using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct FlyRenderer : ISharedComponentData, IEquatable<FlyRenderer>
{
    public Material material;

    public ShadowCastingMode castingShadows;

    public bool receiveShadows;

    public bool Equals(FlyRenderer other)
    {
        return Equals(material, other.material) && castingShadows == other.castingShadows && receiveShadows == other.receiveShadows;
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
            hashCode = (hashCode * 397) ^ (int)castingShadows;
            hashCode = (hashCode * 397) ^ receiveShadows.GetHashCode();
            return hashCode;
        }
    }
}