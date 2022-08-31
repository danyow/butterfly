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

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly.JobSystem
{
    [BurstCompile]
    public unsafe struct ButterflyReconstructionJob: IJobParallelFor, IParticleReconstructionJob
    {
        private const float kSize = 0.005f;

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
            var p = particles[index];

            var az = p.velocity + 0.001f;
            var ax = math.cross(new float3(0, 1, 0), az);
            var ay = math.cross(az, ax);

            var freq = 8 + p.random * 20;
            var flap = math.sin(freq * p.time);

            ax = math.normalize(ax) * kSize;
            ay = math.normalize(ay) * kSize * flap;
            az = math.normalize(az) * kSize;

            var pos = translations[index].Value;
            var face = triangles[index];

            var va1 = pos + face.vertex1;
            var va2 = pos + face.vertex2;
            var va3 = pos + face.vertex3;

            var vb1 = pos + az * 0.2f;
            var vb2 = pos - az * 0.2f;
            var vb3 = pos - ax + ay + az;
            var vb4 = pos - ax + ay - az;
            var vb5 = vb3 + ax * 2;
            var vb6 = vb4 + ax * 2;

            var pt = math.saturate(p.time);
            var v1 = math.lerp(va1, vb1, pt);
            var v2 = math.lerp(va2, vb2, pt);
            var v3 = math.lerp(va3, vb3, pt);
            var v4 = math.lerp(va3, vb4, pt);
            var v5 = math.lerp(va3, vb5, pt);
            var v6 = math.lerp(va3, vb6, pt);

            AddTriangle(v1, v2, v5);
            AddTriangle(v5, v2, v6);
            AddTriangle(v3, v4, v1);
            AddTriangle(v1, v4, v2);
        }
    }

    // public sealed partial class ButterflyParticleSystem: SystemBase
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
    //     protected override void OnCreate()
    //     {
    //         _query = GetEntityQuery(
    //             typeof(Particle),
    //             typeof(Triangle),
    //             typeof(Translation),
    //             typeof(Renderer),         // 共享的
    //             typeof(ButterflyParticle) // 共享的
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
    //             _query.SetSharedComponentFilter(renderer);
    //     
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
    //             var job = new ButterflyReconstructionJob
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