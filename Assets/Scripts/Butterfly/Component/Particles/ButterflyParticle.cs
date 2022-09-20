namespace Butterfly.Component.Particles
{
    [System.SerializableAttribute]
    public struct ButterflyParticle: Unity.Entities.ISharedComponentData, Butterfly.Component.Interface.IParticleVariant
    {
        /// <summary>
        /// 权重
        /// </summary>
        public float weight;
        public float GetWeight() => weight;

        /// <summary>
        /// 存活时间
        /// </summary>
        public float life;
        public float GetLife() => life;

        /// <summary>
        /// 尺寸
        /// </summary>
        public float size;
    }
}