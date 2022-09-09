namespace Butterfly.Component.Particles
{
    [System.SerializableAttribute]
    public struct SpikeParticle: Unity.Entities.ISharedComponentData, Butterfly.Component.Interface.IParticleVariant
    {
        public float weight;
        public float GetWeight() => weight;

        public float life;
        public float GetLife() => life;
    }
}