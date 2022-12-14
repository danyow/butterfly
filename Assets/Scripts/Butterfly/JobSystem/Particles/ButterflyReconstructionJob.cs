using Butterfly.Component;
using Butterfly.JobSystem.Particles.Core;
using Butterfly.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Vector3 = UnityEngine.Vector3;
using Random = Butterfly.Utility.Random;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly.JobSystem.Particles
{
    [BurstCompile]
    public unsafe struct ButterflyReconstructionJob
        : IJobParallelFor, IParticleReconstructionJob<Butterfly.Component.Particles.ButterflyParticle>
    {
        [ReadOnly]
        private NativeArray<Particle> _particles;

        [ReadOnly]
        private NativeArray<Triangle> _triangles;

        [ReadOnly]
        private NativeArray<Translation> _translations;

        [NativeDisableUnsafePtrRestriction]
        private void* _vertices;

        [NativeDisableUnsafePtrRestriction]
        private void* _normals;

        private NativeCounter.Concurrent _counter;
        private Butterfly.Component.Particles.ButterflyParticle _variant;

        public NativeArray<Particle> GetParticles() => _particles;

        public NativeArray<Triangle> GetTriangles() => _triangles;

        public NativeArray<Translation> GetTranslations() => _translations;

        public void Initialize(
            Butterfly.Component.Particles.ButterflyParticle variant,
            EntityQuery query,
            Vector3[] vertices,
            Vector3[] normals,
            NativeCounter.Concurrent counter
        )
        {
            _particles = query.ToComponentDataArray<Particle>(Allocator.TempJob);
            _triangles = query.ToComponentDataArray<Triangle>(Allocator.TempJob);
            _translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);

            _vertices = UnsafeUtility.AddressOf(ref vertices[0]);
            _normals = UnsafeUtility.AddressOf(ref normals[0]);

            _variant = variant;
            _counter = counter;
        }

        public void Execute(int index)
        {
            var particle = _particles[index];

            // ??????????????? lerp ????????????
            var ts = particle.time / (_variant.life * particle.lifeRandom);
            var size = _variant.size * (1 - ts);

            // ??????????????????
            var az = particle.velocity + 0.001f;
            var ax = math.cross(new float3(0, 1, 0), az);
            var ay = math.cross(az, ax);

            // ??????
            var freq = 8 + Random.Value01(particle.id + 10000) * 20;
            var flap = math.sin(freq * particle.time);

            // ?????????
            ax = math.normalize(ax) * size;
            ay = math.normalize(ay) * size * flap;
            az = math.normalize(az) * size;

            // ??????
            var pos = _translations[index].Value;
            var face = _triangles[index];

            var va1 = pos + face.vertex1;
            var va2 = pos + face.vertex2;
            var va3 = pos + face.vertex3;

            var vb1 = pos + az * 0.2f;
            var vb2 = pos - az * 0.2f;
            var vb3 = pos - ax + ay + az;
            var vb4 = pos - ax + ay - az;
            var vb5 = vb3 + ax * 2;
            var vb6 = vb4 + ax * 2;

            var pt = math.saturate(particle.time);
            var v1 = math.lerp(va1, vb1, pt);
            var v2 = math.lerp(va2, vb2, pt);
            var v3 = math.lerp(va3, vb3, pt);
            var v4 = math.lerp(va3, vb4, pt);
            var v5 = math.lerp(va3, vb5, pt);
            var v6 = math.lerp(va3, vb6, pt);

            // ??????
            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v1, v2, v5);
            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v5, v2, v6);
            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v3, v4, v1);
            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v1, v4, v2);
        }
    }
}