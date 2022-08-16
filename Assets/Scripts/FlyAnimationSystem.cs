using System.Collections.Generic;
using FlyComponent;
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

    protected override void OnUpdate()
    {
        EntityManager.GetAllUniqueSharedComponentData(_renderers);

        foreach(var renderer in _renderers)
        {
            var vertices = renderer.vertices;
            var normals = renderer.normals;

            if(renderer.meshInstance == null)
            {
                continue;
            }
            var spawnTime = (float)Time.ElapsedTime;
            Entities
               .ForEach(
                    (int entityInQueryIndex, in Fly fly, in Facet facet, in Translation translation, in Entity entity) =>
                    {
                        var p = translation.Value;
                        var f = facet;
                        var vi = entityInQueryIndex * 3;

                        var v1 = p + f.vertex1;
                        var v2 = p + f.vertex2;
                        var v3 = p + f.vertex3;

                        var offs = new float3(0, 0, spawnTime);
                        v1 *= 1 + noise.cnoise(v1 * 2 + offs) * 0.2f;
                        v2 *= 1 + noise.cnoise(v2 * 2 + offs) * 0.2f;
                        v3 *= 1 + noise.cnoise(v3 * 2 + offs) * 0.2f;

                        var n = math.normalize(math.cross(v2 - v1, v3 - v1));

                        vertices[vi + 0] = v1;
                        vertices[vi + 1] = v2;
                        vertices[vi + 2] = v3;

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