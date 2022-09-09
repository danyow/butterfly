namespace Butterfly.Component.Particles
{
    internal sealed class ButterflyParticleConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (ButterflyParticleAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new Butterfly.Component.Particles.ButterflyParticle
                        {
                            weight = authoring.weight, life = authoring.life, size = authoring.size,
                        }
                    );
                }
            );
        }
    }
}