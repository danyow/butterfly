using System.Collections.Generic;
using Butterfly.Component;
using Butterfly.Utility;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly
{
    internal sealed partial class SimpleParticleSystem: SystemBase
    {
        [BurstCompatible]
        private unsafe struct ReconstructionJob: IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Particle> particles;

            [ReadOnly]
            public NativeArray<Triangle> triangles;

            [ReadOnly]
            public NativeArray<Translation> translations;

            [NativeDisableUnsafePtrRestriction]
            public void* vertices;

            [NativeDisableUnsafePtrRestriction]
            public void* normals;

            public NativeCounter.Concurrent counter;

            private void AddTriangle(float3 v1, float3 v2, float3 v3)
            {
                var i = counter.Increment() * 3;
                UnsafeUtility.WriteArrayElement(vertices, i + 0, v1);
                UnsafeUtility.WriteArrayElement(vertices, i + 1, v2);
                UnsafeUtility.WriteArrayElement(vertices, i + 2, v3);

                var n = math.normalize(math.cross(v2 - v1, v3 - v1));
                UnsafeUtility.WriteArrayElement(normals, i + 0, n);
                UnsafeUtility.WriteArrayElement(normals, i + 1, n);
                UnsafeUtility.WriteArrayElement(normals, i + 2, n);
            }

            public void Execute(int index)
            {
                var pos = translations[index].Value;
                var face = triangles[index];

                var v1 = pos + face.vertex1;
                var v2 = pos + face.vertex2;
                var v3 = pos + face.vertex3;

                AddTriangle(v1, v2, v3);
            }
        }

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
                typeof(Renderer),      // 共享的
                typeof(SimpleParticle) // 共享的
            );
        }

        protected override unsafe void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            for(var i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];

                if(renderer.workMesh == null)
                {
                    continue;
                }

                _query.SetSharedComponentFilter(renderer);
                if(_query.CalculateEntityCount() == 0)
                {
                    continue;
                }

                // 把需要的变量先作为临时变量
                var vertices = (Vector3*)UnsafeUtility.AddressOf(ref renderer.vertices[0]);
                var normals = (Vector3*)UnsafeUtility.AddressOf(ref renderer.normals[0]);
                var counter = renderer.concurrentCounter;

                var job = new ReconstructionJob
                {
                    particles = _query.ToComponentDataArray<Particle>(Allocator.TempJob),
                    triangles = _query.ToComponentDataArray<Triangle>(Allocator.TempJob),
                    translations = _query.ToComponentDataArray<Translation>(Allocator.TempJob),
                    vertices = vertices,
                    normals = normals,
                    counter = counter,
                };
                Dependency = job.Schedule(_query.CalculateEntityCount(), 16, Dependency);

                Dependency = job.particles.Dispose(Dependency);
                Dependency = job.triangles.Dispose(Dependency);
                Dependency = job.translations.Dispose(Dependency);
            }

            _renderers.Clear();
        }

        private unsafe static void AddTriangle(
            float3 v1,
            float3 v2,
            float3 v3,
            NativeCounter.Concurrent counter,
            void* vertices,
            void* normals
        )
        {
            var i = counter.Increment() * 3;

            UnsafeUtility.WriteArrayElement(vertices, i + 0, v1);
            UnsafeUtility.WriteArrayElement(vertices, i + 1, v2);
            UnsafeUtility.WriteArrayElement(vertices, i + 2, v3);

            var n = math.normalize(math.cross(v2 - v1, v3 - v1));

            UnsafeUtility.WriteArrayElement(normals, i + 0, n);
            UnsafeUtility.WriteArrayElement(normals, i + 1, n);
            UnsafeUtility.WriteArrayElement(normals, i + 2, n);
        }
    }
}