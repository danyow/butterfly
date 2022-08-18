using Unity.Entities;

namespace Butterfly.Component
{
    [System.Serializable]
    public struct NoiseEffector: IComponentData
    {
        /// <summary>
        /// 频率
        /// </summary>
        public float frequency;

        /// <summary>
        /// 振幅
        /// </summary>
        public float amplitude;
    }
}