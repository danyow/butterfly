using Butterfly.Component;
using Butterfly.Utility;
using Butterfly.JobSystem.Particles.Core;
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
    public unsafe struct WaveReconstructionJob
        : IJobParallelFor, IParticleReconstructionJob<Butterfly.Component.Particles.WaveParticle>
    {
        [ReadOnly]
        private NativeArray<Particle> _particles;

        [ReadOnly]
        private NativeArray<Triangle> _triangles;

        [ReadOnly]
        private NativeArray<LocalToWorldTransform> _translations;

        [NativeDisableUnsafePtrRestriction]
        private void* _vertices;

        [NativeDisableUnsafePtrRestriction]
        private void* _normals;

        private Butterfly.Component.Particles.WaveParticle _variant;
        private NativeCounter.Concurrent _counter;

        public NativeArray<Particle> GetParticles() => _particles;

        public NativeArray<Triangle> GetTriangles() => _triangles;

        public NativeArray<LocalToWorldTransform> GetTransforms() => _translations;

        public void Initialize(
            Butterfly.Component.Particles.WaveParticle variant,
            EntityQuery query,
            Vector3[] vertices,
            Vector3[] normals,
            NativeCounter.Concurrent counter
        )
        {
            _particles = query.ToComponentDataArray<Particle>(Allocator.TempJob);
            _triangles = query.ToComponentDataArray<Triangle>(Allocator.TempJob);
            _translations = query.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);

            _vertices = UnsafeUtility.AddressOf(ref vertices[0]);
            _normals = UnsafeUtility.AddressOf(ref normals[0]);

            _variant = variant;
            _counter = counter;
        }

        public void Execute(int index)
        {
            var particle = _particles[index];
            var pos = _translations[index].Value.Position;
            var face = _triangles[index];
            var time = (float)particle.elapsedTime;

            var v1 = pos + face.vertex1;
            var v2 = pos + face.vertex2;
            var v3 = pos + face.vertex3;

            var offs = new float3(0, 0, time * 1.6f);
            v1 *= 1 + noise.cnoise(v1 * 2 + offs) * 0.2f;
            v2 *= 1 + noise.cnoise(v2 * 2 + offs) * 0.2f;
            v3 *= 1 + noise.cnoise(v3 * 2 + offs) * 0.2f;

            ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v1, v2, v3);
        }
    }
}