namespace Butterfly.Component
{
    [System.SerializableAttribute]
    internal struct ButterflyParticle: Unity.Entities.ISharedComponentData, IParticleVariant
    {
        public float weight;
        public float GetWeight() => weight;
    }
}