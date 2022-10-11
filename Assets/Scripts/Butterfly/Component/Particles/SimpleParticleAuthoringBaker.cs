namespace Butterfly.Component.Particles
{
    internal sealed class SimpleParticleAuthoringBaker: Unity.Entities.Baker<SimpleParticleAuthoring>
    {
        public override void Bake(SimpleParticleAuthoring authoring)
        {
            AddSharedComponent(new SimpleParticle { weight = authoring.weight, life = authoring.life, });
        }
    }
}