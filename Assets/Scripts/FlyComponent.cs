using Unity.Entities;
using Unity.Mathematics;

public struct Fly: IComponentData
{
    public float life;
}

/// <summary>
/// 刻面?
/// </summary>
public struct Facet: IComponentData
{
    public float3 vertex1;
    public float3 vertex2;
    public float3 vertex3;
}