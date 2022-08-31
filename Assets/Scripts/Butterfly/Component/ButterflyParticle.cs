namespace Butterfly.Component
{
    [System.SerializableAttribute]
    public struct ButterflyParticle: Unity.Entities.ISharedComponentData, IParticleVariant
    {
        public float weight;
        public float GetWeight() => weight;

        public float life;
        public float GetLife() => life;

    }
}