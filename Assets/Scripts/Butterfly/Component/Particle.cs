using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.Component
{
    // 粒子
    public struct Particle: IComponentData
    {
        /// <summary>
        /// 速度
        /// </summary>
        public float3 velocity;

        public uint id;

        public float time;

        public float lifeRandom;
    }
}