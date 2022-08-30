namespace Butterfly.Component
{
    internal sealed class SimpleParticleConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (SimpleParticleAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new SimpleParticle { weight = authoring.weight, }
                    );
                }
            );
        }
    }
}