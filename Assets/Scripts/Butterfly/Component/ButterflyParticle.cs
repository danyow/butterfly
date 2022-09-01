namespace Butterfly.Component
{
    [System.SerializableAttribute]
    public struct ButterflyParticle: Unity.Entities.ISharedComponentData, Butterfly.Component.Interface.IParticleVariant
    {
        public float weight;
        public float GetWeight() => weight;

        public float life;
        public float GetLife() => life;

        public float size;
    }
}