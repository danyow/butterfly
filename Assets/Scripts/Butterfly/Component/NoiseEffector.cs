using Unity.Entities;

namespace Butterfly.Component
{
    [System.Serializable]
    internal struct NoiseEffector: IComponentData
    {
        /// <summary>
        /// 频率
        /// </summary>
        public float frequency;

        /// <summary>
        /// 振幅
        /// </summary>
        public float amplitude;

        /// <summary>
        /// 动画速度
        /// </summary>
        public float animationSpeed;
    }
}