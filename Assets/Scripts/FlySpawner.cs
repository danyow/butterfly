using System;
using Unity.Entities;
using UnityEngine;


[Serializable]
public struct FlySpawner : ISharedComponentData, IEquatable<FlySpawner>
{
    public Mesh templateMesh;

    public bool Equals(FlySpawner other)
    {
        return Equals(templateMesh, other.templateMesh);
    }

    public override bool Equals(object obj)
    {
        return obj is FlySpawner other && Equals(other);
    }

    public override int GetHashCode()
    {
        return templateMesh != null ? templateMesh.GetHashCode() : 0;
    }
}