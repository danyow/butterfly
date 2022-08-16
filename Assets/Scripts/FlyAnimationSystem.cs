using System.Collections.Generic;
using FlyComponent;
using NativeItem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
public partial class FlyAnimationSystem: SystemBase
{
    private readonly List<FlyRenderer> _renderers = new List<FlyRenderer>();

    private EntityQuery _query;

    private Vector3[] _managedVertexArray;
    private Vector3[] _managedNormalArray;
    private int[] _managedIndexArray;

    protected override void OnCreate()
    {
        base.OnCreate();

        _query = GetEntityQuery(typeof(Fly), typeof(Facet), typeof(Translation), typeof(FlyRenderer));
    }

    protected unsafe override void OnUpdate()
    {
        EntityManager.GetAllUniqueSharedComponentData(_renderers);

        for(var i = 0; i < _renderers.Count; i++)
        {
            var renderer = _renderers[i];
            var vertices = renderer.vertices;
            var normals = renderer.normals;

            if(renderer.meshInstance == null)
            {
                continue;
            }
            renderer.counter.Count = 0;
            NativeCounter.Concurrent counter = renderer.counter;

            var spawnTime = (float)Time.ElapsedTime;
            Entities
               .ForEach(
                    (int entityInQueryIndex, in Fly fly, in Facet facet, in Translation translation, in Entity entity) =>
                    {
                        var p = translation.Value;
                        var f = facet;

                        var v1 = p + f.vertex1;
                        var v2 = p + f.vertex2;
                        var v3 = p + f.vertex3;

                        noise.snoise(p, out var d1);

                        v1 += d1 * 0.05f * spawnTime;
                        v2 += d1 * 0.05f * spawnTime;
                        v3 += d1 * 0.05f * spawnTime;

                        var n = math.normalize(math.cross(v2 - v1, v3 - v1));

                        var vi = counter.Increment() * 3;

                        vertices[vi + 0] = v1;
                        vertices[vi + 1] = v2;
                        vertices[vi + 2] = v3;

                        normals[vi + 0] = n;
                        normals[vi + 1] = n;
                        normals[vi + 2] = n;

                        vi = counter.Increment() * 3;

                        vertices[vi + 0] = v1 - new float3(0.5f, 0, 0);
                        vertices[vi + 1] = v2 - new float3(0.5f, 0, 0);
                        vertices[vi + 2] = v3 - new float3(0.5f, 0, 0);

                        normals[vi + 0] = n;
                        normals[vi + 1] = n;
                        normals[vi + 2] = n;
                    }
                )
               .WithNativeDisableParallelForRestriction(vertices)
               .WithNativeDisableParallelForRestriction(normals)
               .WithSharedComponentFilter(renderer)
               .WithStoreEntityQueryInField(ref _query)
               .ScheduleParallel();
        }

        _renderers.Clear();
    }

    [BurstCompile]
    private struct ConstructionJob: IJobParallelFor
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
}