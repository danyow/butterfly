using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Mesh = UnityEngine.Mesh;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

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

    private const int MaxVertices = 60000;

    private unsafe int _copySize = sizeof(float3) * MaxVertices;
    
    public Mesh SharedMesh { get; private set; }

    private EntityQuery _query;
    
    private NativeArray<float3> _vertexCache;
    private NativeArray<float3> _normalCache;
    
    private Vector3[] _managedVertexArray;
    private Vector3[] _managedNormalArray;

    protected override void OnCreate()
    {
        base.OnCreate();

        _query = GetEntityQuery(typeof(Fly), typeof(Translation), typeof(Facet));
        
        SharedMesh = new Mesh();
        
        _vertexCache = new NativeArray<float3>(MaxVertices, Allocator.Persistent);
        _normalCache = new NativeArray<float3>(MaxVertices, Allocator.Persistent);
        
        _managedVertexArray = new Vector3 [MaxVertices];
        _managedNormalArray = new Vector3 [MaxVertices];

        SharedMesh.vertices = _managedVertexArray;
        SharedMesh.normals = _managedNormalArray;
        
        var indexes = new int[MaxVertices];
        for (var i = 0; i < MaxVertices; i++)
        {
            indexes[i] = i;
        }
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

        var vp = UnsafeUtility.AddressOf(ref _managedVertexArray[0]);
        var np = UnsafeUtility.AddressOf(ref _managedNormalArray[0]);
        
        UnsafeUtility.MemCpy(vp, _vertexCache.GetUnsafePtr(), _copySize);
        UnsafeUtility.MemCpy(np, _normalCache.GetUnsafePtr(), _copySize);
        
        SharedMesh.vertices = _managedVertexArray;

        Entities
           .ForEach(
                (int entityInQueryIndex, ref Fly fly, ref Facet facet, ref Translation translation) =>
                {
                    // var vi = entityInQueryIndex * 3;
                    // var p = translation.Value;
                    // _vertexCache[vi + 0] = p;
                    // _vertexCache[vi + 1] = p + new float3(0, 0.1f, 0);
                    // _vertexCache[vi + 2] = p + new float3(0.1f, 0, 0);
                    
                    var p = translation.Value;
                    var f = facet;
                    var vi = entityInQueryIndex * 3;

                    var v1 = p + f.vertex1;
                    var v2 = p + f.vertex2;
                    var v3 = p + f.vertex3;
                    var n = math.normalize(math.cross(v2 - v1, v3 - v1));

                    _vertexCache[vi + 0] = v1;
                    _vertexCache[vi + 1] = v2;
                    _vertexCache[vi + 2] = v3;

                    _normalCache[vi + 0] = n;
                    _normalCache[vi + 1] = n;
                    _normalCache[vi + 2] = n;
                    
                }
            )
           .WithStoreEntityQueryInField(ref _query)
           .WithoutBurst()
           .Run();
    }

}