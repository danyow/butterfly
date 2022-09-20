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
using Random = Butterfly.Utility.Random;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable UnusedType.Local
// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly.JobSystem.Particles
{
    [BurstCompile]
    public unsafe struct SimpleReconstructionJob: IJobParallelFor, Butterfly.JobSystem.Particles.Core.IParticleReconstructionJob<Butterfly.Component.Particles.SimpleParticle>
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

        private Butterfly.Component.Particles.SimpleParticle _variant;
        
        private NativeCounter.Concurrent _counter;

        public NativeArray<Particle> GetParticles() => _particles;

        public NativeArray<Triangle> GetTriangles() => _triangles;

        public NativeArray<Translation> GetTranslations() => _translations;

        public void Initialize(
            Butterfly.Component.Particles.SimpleParticle variant,
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

        private void AddTriangle(float3 v1, float3 v2, float3 v3)
        {
            // 顶点输出
            var i = _counter.Increment() * 3;
            UnsafeUtility.WriteArrayElement(_vertices, i + 0, v1);
            UnsafeUtility.WriteArrayElement(_vertices, i + 1, v2);
            UnsafeUtility.WriteArrayElement(_vertices, i + 2, v3);

            // 法线输出
            var n = math.normalize(math.cross(v2 - v1, v3 - v1));
            UnsafeUtility.WriteArrayElement(_normals, i + 0, n);
            UnsafeUtility.WriteArrayElement(_normals, i + 1, n);
            UnsafeUtility.WriteArrayElement(_normals, i + 2, n);
        }

        public void Execute(int index)
        {
            var particle = _particles[index];
            var face = _triangles[index];

            // 使用简单的 lerp 进行缩放
            var scale = 1 - particle.time / (_variant.life * particle.lifeRandom);

            // 随机旋转
            var fwd = particle.velocity + 1e-4f;
            var axis = math.normalize(math.cross(fwd, face.vertex1));
            var avel = Random.Value01(particle.id + 10000) * 8;
            var rot = quaternion.AxisAngle(axis, particle.time * avel);

            // 顶点位置
            var pos = _translations[index].Value;
            var v1 = pos + math.mul(rot, face.vertex1) * scale;
            var v2 = pos + math.mul(rot, face.vertex2) * scale;
            var v3 = pos + math.mul(rot, face.vertex3) * scale;

            AddTriangle(v1, v2, v3);
        }
    }
}