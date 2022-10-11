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
        private NativeArray<LocalToWorldTransform> _translations;

        [NativeDisableUnsafePtrRestriction]
        private void* _vertices;

        [NativeDisableUnsafePtrRestriction]
        private void* _normals;

        private Butterfly.Component.Particles.SpikeParticle _variant;
        private NativeCounter.Concurrent _counter;

        public NativeArray<Particle> GetParticles() => _particles;

        public NativeArray<Triangle> GetTriangles() => _triangles;

        public NativeArray<LocalToWorldTransform> GetTransforms() => _translations;

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
            _translations = query.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);

            _vertices = UnsafeUtility.AddressOf(ref vertices[0]);
            _normals = UnsafeUtility.AddressOf(ref normals[0]);

            _variant = variant;
            _counter = counter;
        }

        public void Execute(int index)
        {
            // var particle = _particles[index];
            // var pos = _translations[index].Value;
            // var face = _triangles[index];
            // var normal = ReconstructionJobUtility.MakeNormal(face.vertex1, face.vertex2, face.vertex3);
            //
            // var time = (float)particle.elapsedTime;
            // var timeScale = math.clamp(particle.time, 0, 1);
            //
            // var offs = new float3(0, time, 0);
            // var d = noise.snoise(pos * 8 + offs);
            // d = math.pow(math.abs(d), 5);
            //
            // var v1 = pos + face.vertex1;
            // var v2 = pos + face.vertex2;
            // var v3 = pos + face.vertex3;
            // var v4 = pos + normal * d * timeScale;
            //
            // ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v1, v2, v4);
            // ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v2, v3, v4);
            // ReconstructionJobUtility.AddTriangle(_counter, _vertices, _normals, v3, v1, v4);

            //////////////////////////////////////////////////////////////////////////////////////////

            var particle = _particles[index];
            var time = (float)particle.elapsedTime;

            var triangleExtent = 0.3f;
            var noiseFrequency = 2.2f;
            var noiseAmplitude = 0.85;
            var noiseAnimation = new float3(0, 0.13f, 0.51f);
            var noiseOffset = noiseAnimation * time;

            // 三个随机顶点
            // var vi = (uint)_counter.Increment() * 3;
            var vi = index * 3;
            var v1 = Random.RandomPoint(vi + 0);
            var v2 = Random.RandomPoint(vi + 1);
            var v3 = Random.RandomPoint(vi + 2);

            // 三角形大小规范化
            v2 = math.normalize(v1 + math.normalize(v2 - v1) * triangleExtent);
            v3 = math.normalize(v1 + math.normalize(v3 - v1) * triangleExtent);

            // 
            var l1 = Noise.SimplexNoise(v1 * noiseFrequency + noiseOffset);
            var l2 = Noise.SimplexNoise(v2 * noiseFrequency + noiseOffset);
            var l3 = Noise.SimplexNoise(v3 * noiseFrequency + noiseOffset);

            v1 *= new float3(1 + math.abs(l1 * l1 * l1) * noiseAmplitude);
            v2 *= new float3(1 + math.abs(l2 * l2 * l2) * noiseAmplitude);
            v3 *= new float3(1 + math.abs(l3 * l3 * l3) * noiseAmplitude);

            // 顶点输出
            // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, (int)(vi + 0), v1);
            // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, (int)(vi + 1), v2);
            // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, (int)(vi + 2), v3);
            //
            // 法线输出
            var n = math.normalize(math.cross(v2 - v1, v3 - v1));

            // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_normals, (int)(vi + 0), n);
            // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_normals, (int)(vi + 1), n);
            // Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_normals, (int)(vi + 2), n);
            WriteVertex(vi + 0, v1, n);
            WriteVertex(vi + 1, v2, n);
            WriteVertex(vi + 2, v3, n);
        }

        private void WriteVertex(uint vidx, float3 p, float3 n)
        {
            var addr_p = vidx * 6 * 4;
            var addr_n = addr_p + 3 * 4;
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, (int)addr_p, p);
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, (int)addr_n, n);

            // Vertices.Store3(addr_p, asuint(p));
            // Vertices.Store3(addr_n, asuint(n));
        }
        private void WriteVertex(int vidx, float3 p, float3 n)
        {
            var addr_p = vidx * 6 * 4;
            var addr_n = addr_p + 3 * 4;
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, addr_p, p);
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(_vertices, addr_n, n);

            // Vertices.Store3(addr_p, asuint(p));
            // Vertices.Store3(addr_n, asuint(n));
        }
    }
}