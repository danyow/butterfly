using Butterfly.Component;
using Butterfly.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly.JobSystem
{
    [BurstCompile]
    public unsafe struct SimpleReconstructionJob: IJobParallelFor, IParticleReconstructionJob
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

        public NativeArray<Particle> GetParticles() => particles;

        public NativeArray<Triangle> GetTriangles() => triangles;

        public NativeArray<Translation> GetTranslations() => translations;

        public void Initialize(EntityQuery query, Vector3[] vertices, Vector3[] normals, NativeCounter.Concurrent counter)
        {
            particles = query.ToComponentDataArray<Particle>(Allocator.TempJob);
            translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
            triangles = query.ToComponentDataArray<Triangle>(Allocator.TempJob);

            // query.ToComponentDataArray<ButterflyParticle>(Allocator.TempJob);
            this.vertices = UnsafeUtility.AddressOf(ref vertices[0]);
            this.normals = UnsafeUtility.AddressOf(ref normals[0]);
            this.counter = counter;
        }

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
            var particle = particles[index];
            var face = triangles[index];

            var fwd = particle.velocity + 1e-4f;
            var axis = math.normalize(math.cross(fwd, face.vertex1));
            var rot = AxisAngle(axis, particle.time * 3);

            var pos = translations[index].Value;
            var v1 = pos + math.mul(rot, face.vertex1);
            var v2 = pos + math.mul(rot, face.vertex2);
            var v3 = pos + math.mul(rot, face.vertex3);

            AddTriangle(v1, v2, v3);
        }

        private static quaternion AxisAngle(float3 axis, float angle)
        {
            var axisUnit = math.normalize(axis);
            var sina = math.sin(0.5f * angle);
            var cosa = math.cos(0.5f * angle);
            return new quaternion { value = new float4(axisUnit.x * sina, axisUnit.y * sina, axisUnit.z * sina, cosa) };
        }
    }

    // internal sealed partial class SimpleParticleSystem: SystemBase
    // {
    //
    //     private readonly List<Renderer> _renderers = new List<Renderer>();
    //
    //     private EntityQuery _query;
    //
    //     private Vector3[] _managedVertexArray;
    //     private Vector3[] _managedNormalArray;
    //     private int[] _managedIndexArray;
    //
    //     private const float kSize = 0.005f;
    //
    //     protected override void OnCreate()
    //     {
    //         _query = GetEntityQuery(
    //             typeof(Particle),
    //             typeof(Triangle),
    //             typeof(Translation),
    //             typeof(Renderer),      // 共享的
    //             typeof(SimpleParticle) // 共享的
    //         );
    //     }
    //
    //     protected override unsafe void OnUpdate()
    //     {
    //         EntityManager.GetAllUniqueSharedComponentData(_renderers);
    //
    //         for(var i = 0; i < _renderers.Count; i++)
    //         {
    //             var renderer = _renderers[i];
    //
    //             // if(renderer.workMesh == null)
    //             // {
    //             //     continue;
    //             // }
    //
    //             _query.SetSharedComponentFilter(renderer);
    //             var count = _query.CalculateEntityCount();
    //             if(count == 0)
    //             {
    //                 continue;
    //             }
    //
    //             // 把需要的变量先作为临时变量
    //             var vertices = (Vector3*)UnsafeUtility.AddressOf(ref renderer.vertices[0]);
    //             var normals = (Vector3*)UnsafeUtility.AddressOf(ref renderer.normals[0]);
    //             var counter = renderer.concurrentCounter;
    //
    //             var job = new ReconstructionJob
    //             {
    //                 particles = _query.ToComponentDataArray<Particle>(Allocator.TempJob),
    //                 triangles = _query.ToComponentDataArray<Triangle>(Allocator.TempJob),
    //                 translations = _query.ToComponentDataArray<Translation>(Allocator.TempJob),
    //                 vertices = vertices,
    //                 normals = normals,
    //                 counter = counter,
    //             };
    //             Dependency = job.Schedule(count, 8, Dependency);
    //
    //             Dependency = job.particles.Dispose(Dependency);
    //             Dependency = job.triangles.Dispose(Dependency);
    //             Dependency = job.translations.Dispose(Dependency);
    //         }
    //
    //         _renderers.Clear();
    //     }
    //
    //     private unsafe static void AddTriangle(
    //         float3 v1,
    //         float3 v2,
    //         float3 v3,
    //         NativeCounter.Concurrent counter,
    //         void* vertices,
    //         void* normals
    //     )
    //     {
    //         var i = counter.Increment() * 3;
    //
    //         UnsafeUtility.WriteArrayElement(vertices, i + 0, v1);
    //         UnsafeUtility.WriteArrayElement(vertices, i + 1, v2);
    //         UnsafeUtility.WriteArrayElement(vertices, i + 2, v3);
    //
    //         var n = math.normalize(math.cross(v2 - v1, v3 - v1));
    //
    //         UnsafeUtility.WriteArrayElement(normals, i + 0, n);
    //         UnsafeUtility.WriteArrayElement(normals, i + 1, n);
    //         UnsafeUtility.WriteArrayElement(normals, i + 2, n);
    //     }
    // }
}