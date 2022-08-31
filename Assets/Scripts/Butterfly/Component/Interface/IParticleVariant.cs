namespace Butterfly.Component.Interface
{
    public interface IParticleVariant
    {
        // 获取权重
        float GetWeight();

        /// <summary>
        /// 获取存活时间
        /// </summary>
        /// <returns></returns>
        float GetLife();
    }
}