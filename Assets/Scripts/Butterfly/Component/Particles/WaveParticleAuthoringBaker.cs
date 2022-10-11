namespace Butterfly.Component.Particles
{
    internal sealed class WaveParticleAuthoringBaker: Unity.Entities.Baker<WaveParticleAuthoring>
    {
        public override void Bake(WaveParticleAuthoring authoring)
        {
            AddSharedComponent(new WaveParticle { weight = authoring.weight, life = authoring.life, });
        }
    }
}