using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.Component
{
    // 粉碎机
    internal struct Particle: IComponentData
    {
        /// <summary>
        /// 速度
        /// </summary>
        public float3 velocity;

        public float time;

        public float random;
    }
}