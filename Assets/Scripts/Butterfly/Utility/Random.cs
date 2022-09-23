using Unity.Mathematics;

// ReSharper disable MemberCanBePrivate.Global

namespace Butterfly.Utility
{
    public static class Random
    {
        // 哈希函数来自 H. Schechter & R. Bridson, goo.gl/RXiKaH
        public static uint Hash(uint s)
        {
            s ^= 2747636419u;
            s *= 2654435769u;
            s ^= s >> 16;
            s *= 2654435769u;
            s ^= s >> 16;
            s *= 2654435769u;
            return s;
        }

        public static float Value01(uint seed)
        {
            return Hash(seed) / 4294967295.0f; // 2^32-1
        }

        /// <summary>
        /// 均匀分布的随机点
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static float3 RandomPoint(uint seed)
        {
            seed *= 2;
            var u = Hash(seed) * math.PI * 2;
            var z = Hash(seed + 1) * 2 - 1;
            var w = math.sqrt(1 - z * z);
            return new float3(math.cos(u) * w, math.sin(u) * w, z);
        }
    }
}