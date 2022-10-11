namespace Butterfly.Component.Particles
{
    internal sealed class ButterflyParticleAuthoringBaker: Unity.Entities.Baker<ButterflyParticleAuthoring>
    {
        public override void Bake(ButterflyParticleAuthoring authoring)
        {
            AddSharedComponent(
                new Butterfly.Component.Particles.ButterflyParticle { weight = authoring.weight, life = authoring.life, size = authoring.size, }
            );
        }
    }
}