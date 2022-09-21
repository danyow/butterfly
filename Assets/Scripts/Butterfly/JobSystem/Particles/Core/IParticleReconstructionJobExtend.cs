using Unity.Mathematics;

namespace Butterfly.JobSystem.Particles.Core
{
    public static class IParticleReconstructionJobExtend
    {
        /// <summary>
        /// 添加三角形
        /// </summary>
        /// <param name="_"></param>
        /// <param name="counter"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public unsafe static void AddTriangle<T>(
            this IParticleReconstructionJob<T> _,
            Butterfly.Utility.NativeCounter.Concurrent counter,
            void* vertices,
            void* normals,
            float3 v1,
            float3 v2,
            float3 v3
        )
        {
            // 顶点输出
            var i = counter.Increment() * 3;
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(vertices, i + 0, v1);
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(vertices, i + 1, v2);
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(vertices, i + 2, v3);

            // 法线输出
            var n = math.normalize(math.cross(v2 - v1, v3 - v1));
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(normals, i + 0, n);
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(normals, i + 1, n);
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.WriteArrayElement(normals, i + 2, n);
        }

        public static float3 MakeNormal<T>(this IParticleReconstructionJob<T> _, float3 a, float3 b, float3 c)
        {
            return math.normalize(math.cross(b - a, c - a));
        }
    }
}