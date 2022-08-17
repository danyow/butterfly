using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.Component
{
    // 粉碎机
    public struct Particle: IComponentData
    {
        /// <summary>
        /// 速度
        /// </summary>
        public float3 velocity;

        public float life;

        public float random;
    }
}