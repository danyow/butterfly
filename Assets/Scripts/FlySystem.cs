using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class FlySystem : SystemBase
{

    private struct ConstructionJob : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<Fly> flies;

        [ReadOnly]
        public NativeArray<Translation> translations;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> vertices;

        public void Execute(int index)
        {
            var vi = index * 3;
            var p = translations[index].Value;
            vertices[vi + 0] = p;
            vertices[vi + 1] = p + new float3(0, 0.1f, 0);
            vertices[vi + 2] = p + new float3(0.1f, 0, 0);
        }
    }

    public Mesh SharedMesh { get; private set; }

    private EntityQuery _query;
    private NativeArray<float3> _vertexCache;
    private Vector3[] _managedVertexArray;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        _query = GetEntityQuery(typeof(Fly), typeof(Translation));
        SharedMesh = new Mesh();
        _vertexCache = new NativeArray<float3>(60000, Allocator.Persistent);
        _managedVertexArray = new Vector3 [_vertexCache.Length];

        var indexes = new int[_vertexCache.Length];
        for (var i = 0; i < indexes.Length; i++)
        {
            indexes[i] = i;
        }
        SharedMesh.vertices = _managedVertexArray;
        SharedMesh.SetTriangles(indexes, 0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Object.Destroy(SharedMesh);
        _vertexCache.Dispose();
        _managedVertexArray = null;
    }

    protected override unsafe void OnUpdate()
    {

        UnsafeUtility.MemCpy(
            UnsafeUtility.AddressOf(ref _managedVertexArray[0]),
            _vertexCache.GetUnsafePtr(),
            sizeof(Vector3) * _managedVertexArray.Length
        );
        SharedMesh.vertices = _managedVertexArray;
        
        Entities
           .ForEach(
            (int entityInQueryIndex, ref Fly fly, ref Translation translation) =>
            {
                var vi = entityInQueryIndex * 3;
                var p = translation.Value;
                _vertexCache[vi + 0] = p;
                _vertexCache[vi + 1] = p + new float3(0, 0.1f, 0);
                _vertexCache[vi + 2] = p + new float3(0.1f, 0, 0);
            })
           .WithStoreEntityQueryInField(ref _query)
           .WithoutBurst()
           .Run();
    }

}