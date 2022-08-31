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
    public unsafe struct SimpleReconstructionJob: IJobParallelFor, IParticleReconstructionJob<SimpleParticle>
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

        private SimpleParticle _variant;
        private NativeCounter.Concurrent _counter;

        public NativeArray<Particle> GetParticles() => _particles;

        public NativeArray<Triangle> GetTriangles() => _triangles;

        public NativeArray<Translation> GetTranslations() => _translations;

        public void Initialize(
            SimpleParticle variant,
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
            var i = _counter.Increment() * 3;
            UnsafeUtility.WriteArrayElement(_vertices, i + 0, v1);
            UnsafeUtility.WriteArrayElement(_vertices, i + 1, v2);
            UnsafeUtility.WriteArrayElement(_vertices, i + 2, v3);

            var n = math.normalize(math.cross(v2 - v1, v3 - v1));
            UnsafeUtility.WriteArrayElement(_normals, i + 0, n);
            UnsafeUtility.WriteArrayElement(_normals, i + 1, n);
            UnsafeUtility.WriteArrayElement(_normals, i + 2, n);
        }

        public void Execute(int index)
        {
            var particle = _particles[index];
            var face = _triangles[index];

            var life = particle.lifeRandom * _variant.life;
            var time = particle.time;
            var scale = 1 - time / life;
            
            var fwd = particle.velocity + 1e-4f;
            var axis = math.normalize(math.cross(fwd, face.vertex1));
            var rot = AxisAngle(axis, particle.time * 3);

            var pos = _translations[index].Value;
            var v1 = pos + math.mul(rot, face.vertex1) * scale;
            var v2 = pos + math.mul(rot, face.vertex2) * scale;
            var v3 = pos + math.mul(rot, face.vertex3) * scale;

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
}