using System.Collections.Generic;
using Butterfly.Component;
using Butterfly.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Butterfly.Utility.Random;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly
{
    [UpdateAfter(typeof(DisintegratorAnimationSystem))]
    public partial class DisintegratorReconstructionSystem: SystemBase
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

                Entities
                   .ForEach(
                        (int entityInQueryIndex, in Facet facet, in Translation translation, in Disintegrator disintegrator) =>
                        {
                            var p = translation.Value;
                            var t = disintegrator.life;

                            var vz = math.normalize(disintegrator.velocity + 0.001f);
                            var vx = math.normalize(math.cross(new float3(0, 1, 0), vz));
                            var vy = math.cross(vz, vx);

                            var f = facet;

                            var freq = 8 + Random.Value01((uint)entityInQueryIndex) * 20;
                            vx *= 0.01f;
                            vy *= 0.01f * math.sin(freq * t);
                            vz *= 0.01f;

                            var v1 = p;
                            var v2 = p - vx - vz + vy;
                            var v3 = p - vx + vz + vy;
                            var v4 = p + vx + vz + vy;
                            var v5 = p + vx - vz + vy;

                            var tf = math.saturate(t);
                            v1 = math.lerp(p + f.vertex1, v1, tf);
                            v2 = math.lerp(p + f.vertex2, v2, tf);
                            v3 = math.lerp(p + f.vertex3, v3, tf);
                            v4 = math.lerp(p + f.vertex2, v4, tf);
                            v5 = math.lerp(p + f.vertex3, v5, tf);

                            AddTriangle(v1, v2, v3, ref counter, ref vertices, ref normals);
                            AddTriangle(v1, v3, v2, ref counter, ref vertices, ref normals);
                            AddTriangle(v1, v4, v5, ref counter, ref vertices, ref normals);
                            AddTriangle(v1, v5, v4, ref counter, ref vertices, ref normals);
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
            ref NativeCounter.Concurrent counter,
            ref NativeArray<float3> vertices,
            ref NativeArray<float3> normals
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