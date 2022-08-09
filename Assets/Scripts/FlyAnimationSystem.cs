using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Mesh = UnityEngine.Mesh;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

public partial class FlyAnimationSystem : SystemBase
{

    [BurstCompile]
    private struct ConstructionJob : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<Fly> flies;
        
        [ReadOnly]
        public NativeArray<Facet> facets;

        [ReadOnly]
        public NativeArray<Translation> translations;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> vertices;
        
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> normals;
        

        public void Execute(int index)
        {
            var p = translations[index].Value;
            var f = facets[index];
            var vi = index * 3;

            var v1 = p + f.vertex1;
            var v2 = p + f.vertex2;
            var v3 = p + f.vertex3;
            var n = math.normalize(math.cross(v2 - v1, v3 - v1));

            vertices[vi + 0] = v1;
            vertices[vi + 1] = v2;
            vertices[vi + 2] = v3;

            normals[vi + 0] = n;
            normals[vi + 1] = n;
            normals[vi + 2] = n;
        }
    }


    internal class MeshCache
    {
        public NativeArray<float3> vertices;
        public NativeArray<float3> normals;
        public FlyRenderer rendererSettings;
        public Mesh meshInstance;

        public MeshCache()
        {
            vertices = new NativeArray<float3>(MaxVertices, Allocator.Persistent);
            normals = new NativeArray<float3>(MaxVertices, Allocator.Persistent);
            meshInstance = new Mesh();
        }

        public void Release()
        {
            vertices.Dispose();
            normals.Dispose();
            Object.Destroy(meshInstance);
        }
    }

    private readonly List<MeshCache> _meshCaches = new List<MeshCache>();
    private readonly List<FlyRenderer> _rendererDataList = new List<FlyRenderer>();

    private const int MaxVertices = 60000;

    private unsafe readonly int _copySize = sizeof(float3) * MaxVertices;

    private EntityQuery _query;
    
    private Vector3[] _managedVertexArray;
    private Vector3[] _managedNormalArray;
    private int[] _managedIndexArray;

    protected override void OnCreate()
    {
        base.OnCreate();

        _query = GetEntityQuery(typeof(Fly), typeof(Translation), typeof(Facet), typeof(FlyRenderer));
        
        _managedVertexArray = new Vector3 [MaxVertices];
        _managedNormalArray = new Vector3 [MaxVertices];
        _managedIndexArray = new int[MaxVertices];

        for (var i = 0; i < MaxVertices; i++)
        {
            _managedIndexArray[i] = i;
        }
    }

    protected override void OnDestroy()
    {
        foreach (var meshCache in _meshCaches)
        {
            meshCache.Release();
        }
        _meshCaches.Clear();
        
        _managedVertexArray = null;
        _managedNormalArray = null;
        _managedIndexArray = null;
    }

    protected override unsafe void OnUpdate()
    {

        var matrix = UnityEngine.Matrix4x4.identity;

        foreach (var cache in _meshCaches)
        {
            var vp = UnsafeUtility.AddressOf(ref _managedVertexArray[0]);
            var np = UnsafeUtility.AddressOf(ref _managedNormalArray[0]);
            
            UnsafeUtility.MemCpy(vp, cache.vertices.GetUnsafePtr(), _copySize);
            UnsafeUtility.MemCpy(np, cache.normals.GetUnsafePtr(), _copySize);

            cache.meshInstance.vertices = _managedVertexArray;
            cache.meshInstance.normals = _managedNormalArray;
            cache.meshInstance.triangles = _managedIndexArray;
            
            UnityEngine.Graphics.DrawMesh(cache.meshInstance, matrix, cache.rendererSettings.material, 0);
        }


        EntityManager.GetAllUniqueSharedComponentData(_rendererDataList);

        var cacheCount = 0;
        for (var i = 0; i < _rendererDataList.Count; i++)
        {
            if (_rendererDataList[i].material == null)
            {
                continue;
            }

            if (cacheCount >= _meshCaches.Count)
            {
                _meshCaches.Add(new MeshCache());
            }

            var cache = _meshCaches[cacheCount++];
            cache.rendererSettings = _rendererDataList[i];
            
            _query.SetSharedComponentFilter(_rendererDataList[i]);
            
            var renderIndex = i;
            Entities
               .ForEach(
                    (int entityInQueryIndex, ref Fly fly, ref Facet facet, ref Translation translation) =>
                    {
                        var p = translation.Value;
                        var f = facet;
                        var vi = entityInQueryIndex * 3;

                        var v1 = p + f.vertex1;
                        var v2 = p + f.vertex2;
                        var v3 = p + f.vertex3;
                        var n = math.normalize(math.cross(v2 - v1, v3 - v1));

                        _meshCaches[renderIndex].vertices[vi + 0] = v1;
                        _meshCaches[renderIndex].vertices[vi + 1] = v2;
                        _meshCaches[renderIndex].vertices[vi + 2] = v3;

                        _meshCaches[renderIndex].normals[vi + 0] = n;
                        _meshCaches[renderIndex].normals[vi + 1] = n;
                        _meshCaches[renderIndex].normals[vi + 2] = n;

                    }
                )
               .WithStoreEntityQueryInField(ref _query)
               .WithoutBurst()
               .Run();

        }
        while (cacheCount > _meshCaches.Count)
        {
            var i = _meshCaches.Count - 1;
            _meshCaches[i].Release();
            _meshCaches.RemoveAt(i);
        }
        
        _rendererDataList.Clear();
    }
}