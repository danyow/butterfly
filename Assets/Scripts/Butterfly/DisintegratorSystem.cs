using System.Collections.Generic;
using Butterfly.Component;
using Butterfly.NativeItem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly
{
    public partial class DisintegratorSystem: SystemBase
    {
        private readonly List<Renderer> _renderers = new List<Renderer>();

        private EntityQuery _query;

        private Vector3[] _managedVertexArray;
        private Vector3[] _managedNormalArray;
        private int[] _managedIndexArray;

        protected override void OnCreate()
        {
            base.OnCreate();

            _query = GetEntityQuery(typeof(Disintegrator), typeof(Facet), typeof(Translation), typeof(Renderer));
        }

        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            for(var i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];
                var vertices = renderer.vertices;
                var normals = renderer.normals;

                if(renderer.workMesh == null)
                {
                    continue;
                }
                renderer.counter.Count = 0;
                NativeCounter.Concurrent counter = renderer.counter;

                var spawnTime = (float)Time.ElapsedTime;
                Entities
                   .ForEach(
                        (int entityInQueryIndex, in Disintegrator disintegrator, in Facet facet, in Translation translation, in Entity entity) =>
                        {
                            var p = translation.Value;
                            var f = facet;
                            var n = MakeNormal(f.vertex1, f.vertex2, f.vertex3);

                            var offs = new float3(0, spawnTime, 0);
                            var d = noise.snoise(p * 8 + offs);
                            d = math.pow(math.abs(d), 5);

                            var v1 = p + f.vertex1;
                            var v2 = p + f.vertex2;
                            var v3 = p + f.vertex3;
                            var v4 = p + n * d;

                            AddTriangle(v1, v2, v4, counter, vertices, normals);
                            AddTriangle(v2, v3, v4, counter, vertices, normals);
                            AddTriangle(v3, v1, v4, counter, vertices, normals);
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

        private static float3 MakeNormal(float3 a, float3 b, float3 c)
        {
            return math.normalize(math.cross(b - a, c - a));
        }

        private static void AddTriangle(
            float3 v1,
            float3 v2,
            float3 v3,
            NativeCounter.Concurrent counter,
            NativeArray<float3> vertices,
            NativeArray<float3> normals
        )
        {
            var n = MakeNormal(v1, v2, v3);
            var vi = counter.Increment() * 3;

            vertices[vi + 0] = v1;
            vertices[vi + 1] = v2;
            vertices[vi + 2] = v3;

            normals[vi + 0] = n;
            normals[vi + 1] = n;
            normals[vi + 2] = n;
        }
    }
}