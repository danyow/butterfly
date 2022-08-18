using System.Collections.Generic;
using Butterfly.Component;
using Butterfly.Utility;
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
                        (int entityInQueryIndex, in Triangle triangle, in Translation translation, in Particle particle) =>
                        {
                            var p = particle;

                            var az = particle.velocity + 0.001f;
                            var ax = math.cross(new float3(0, 1, 0), az);
                            var ay = math.cross(az, ax);

                            var freq = 8 + p.random * 20;
                            var flap = math.sin(freq * p.life);

                            ax = math.normalize(ax) * kSize;
                            ay = math.normalize(ay) * kSize * flap;
                            az = math.normalize(az) * kSize;

                            var pos = translation.Value;
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

                            var pt = math.saturate(p.life);
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

        private static void AddTriangle(
            float3 v1,
            float3 v2,
            float3 v3,
            NativeCounter.Concurrent counter,
            NativeArray<float3> vertices,
            NativeArray<float3> normals
        )
        {
            var i = counter.Increment() * 3;

            vertices[i + 0] = v1;
            vertices[i + 1] = v2;
            vertices[i + 2] = v3;

            normals[i + 0] = normals[i + 1] = normals[i + 2] = math.normalize(math.cross(v2 - v1, v3 - v1));
        }
    }
}