namespace Butterfly.Component.Particles
{
    internal sealed class WaveParticleConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (WaveParticleAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new WaveParticle { weight = authoring.weight, life = authoring.life, }
                    );
                }
            );
        }
    }
}