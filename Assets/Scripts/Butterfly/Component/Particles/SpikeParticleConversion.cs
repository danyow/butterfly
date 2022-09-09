namespace Butterfly.Component.Particles
{
    internal sealed class SpikeParticleConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (SimpleParticleAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new SpikeParticle { weight = authoring.weight, life = authoring.life, }
                    );
                }
            );
        }
    }
}