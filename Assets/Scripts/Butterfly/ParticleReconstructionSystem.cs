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
    [UpdateAfter(typeof(ParticleAnimationSystem))]
    public partial class ParticleReconstructionSystem: SystemBase
    {
        private readonly List<Renderer> _renderers = new List<Renderer>();

        private EntityQuery _query;

        private Vector3[] _managedVertexArray;
        private Vector3[] _managedNormalArray;
        private int[] _managedIndexArray;

        private const float kSize = 0.005f;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                typeof(Particle),
                typeof(Triangle),
                typeof(Translation),
                typeof(Renderer) // 共享的
            );
        }

        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            for(var i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];

                if(renderer.workMesh == null)
                {
                    continue;
                }

                // 重置三角计数器。
                renderer.counter.Count = 0;

                // 把需要的变量先作为临时变量
                var vertices = renderer.vertices;
                var normals = renderer.normals;
                NativeCounter.Concurrent counter = renderer.counter;

                Entities
                   .ForEach(
                        (int entityInQueryIndex, in Triangle triangle, in Translation translation, in Particle disintegrator) =>
                        {
                            var pos = translation.Value;
                            var time = disintegrator.life;

                            var freq = 8 + Random.Value01((uint)entityInQueryIndex) * 20;
                            var flap = math.sin(freq * time);

                            var az = math.normalize(disintegrator.velocity + 0.001f);
                            var ax = math.normalize(math.cross(new float3(0, 1, 0), az));
                            var ay = math.cross(az, ax);

                            ax = math.normalize(ax) * kSize;
                            ay = math.normalize(ay) * kSize * flap;
                            az = math.normalize(az) * kSize;

                            var face = triangle;
                            var va1 = pos + face.vertex1;
                            var va2 = pos + face.vertex2;
                            var va3 = pos + face.vertex3;

                            var vb1 = pos + az * 0.2f;
                            var vb2 = pos - az * 0.2f;
                            var vb3 = pos - ax + ay + az;
                            var vb4 = pos - ax + ay - az;
                            var vb5 = vb3 + ax * 2;
                            var vb6 = vb4 + ax * 2;

                            var pt = math.saturate(time);
                            var v1 = math.lerp(va1, vb1, pt);
                            var v2 = math.lerp(va2, vb2, pt);
                            var v3 = math.lerp(va3, vb3, pt);
                            var v4 = math.lerp(va3, vb4, pt);
                            var v5 = math.lerp(va3, vb5, pt);
                            var v6 = math.lerp(va3, vb6, pt);

                            AddTriangle(v1, v2, v5, counter, vertices, normals);
                            AddTriangle(v5, v2, v6, counter, vertices, normals);
                            AddTriangle(v3, v4, v1, counter, vertices, normals);
                            AddTriangle(v1, v4, v2, counter, vertices, normals);
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