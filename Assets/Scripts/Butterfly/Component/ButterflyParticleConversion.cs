namespace Butterfly.Component
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
                        new ButterflyParticle { weight = authoring.weight, }
                    );
                }
            );
        }
    }
}