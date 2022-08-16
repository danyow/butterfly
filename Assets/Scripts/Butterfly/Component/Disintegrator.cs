using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.Component
{
    // 粉碎机
    public struct Disintegrator: IComponentData
    {
        public float life;
        /// <summary>
        /// 速度
        /// </summary>
        public float3 velocity;
    }
}