using Unity.Entities;

[System.Serializable]
public struct FlySpawner: ISharedComponentData, System.IEquatable<FlySpawner>
{
    public UnityEngine.Mesh templateMesh;

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