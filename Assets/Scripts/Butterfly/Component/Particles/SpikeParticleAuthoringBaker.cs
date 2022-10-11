namespace Butterfly.Component.Particles
{
    internal sealed class SpikeParticleAuthoringBaker: Unity.Entities.Baker<SpikeParticleAuthoring>
    {
        public override void Bake(SpikeParticleAuthoring authoring)
        {
            AddSharedComponent(new SpikeParticle { weight = authoring.weight, life = authoring.life, });
        }
    }
}