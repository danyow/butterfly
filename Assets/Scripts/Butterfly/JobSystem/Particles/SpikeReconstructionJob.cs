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

namespace Butterfly.JobSystem.Particles
{
    [BurstCompile]
    public unsafe struct SpikeReconstructionJob
        : IJobParallelFor, IParticleReconstructionJob<Butterfly.Component.Particles.SpikeParticle>
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

        private Butterfly.Component.Particles.SpikeParticle _variant;
        private NativeCounter.Concurrent _counter;

        public NativeArray<Particle> GetParticles() => _particles;

        public NativeArray<Triangle> GetTriangles() => _triangles;

        public NativeArray<Translation> GetTranslations() => _translations;

        public void Initialize(
            Butterfly.Component.Particles.SpikeParticle variant,
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
            var pos = _translations[index].Value;
            var face = _triangles[index];
            var normal = ReconstructionJobUtility.MakeNormal(face.vertex1, face.vertex2, face.vertex3);

            var time = (float)particle.elapsedTime;
            var timeScale = math.clamp(particle.time, 0, 1);

            var offs = new float3(0, time, 0);
            var d = noise.snoise(pos * 8 + offs);
            d = math.pow(math.abs(d), 5);

            var v1 = pos + face.vertex1;
            var v2 = pos + face.vertex2;
            var v3 = pos + face.vertex3;
            var v4 = pos + normal * d * timeScale;

            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v1, v2, v4);
            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v2, v3, v4);
            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v3, v1, v4);
        }
    }
}