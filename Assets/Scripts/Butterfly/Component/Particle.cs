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

        /// <summary>
        /// id
        /// </summary>
        public uint id;

        /// <summary>
        /// 时间
        /// </summary>
        public float time;
        
        /// <summary>
        /// 经过时间
        /// </summary>
        public double elapsedTime;

        /// <summary>
        /// 随机生命
        /// </summary>
        public float lifeRandom;

        /// <summary>
        /// 特效率
        /// </summary>
        public float effectRate;
    }
}