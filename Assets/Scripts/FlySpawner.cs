using System;
using Unity.Entities;
using UnityEngine;


[Serializable]
public struct FlySpawner : ISharedComponentData, IEquatable<FlySpawner>
{
    public Mesh tempMesh;

    public bool Equals(FlySpawner other)
    {
        return Equals(tempMesh, other.tempMesh);
    }

    public override bool Equals(object obj)
    {
        return obj is FlySpawner other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (tempMesh != null ? tempMesh.GetHashCode() : 0);
    }
}