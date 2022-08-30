namespace Butterfly.Component
{
    [System.SerializableAttribute]
    internal struct SimpleParticle: Unity.Entities.ISharedComponentData, IParticleVariant
    {
        public float weight;
        public float GetWeight() => weight;
    }
}