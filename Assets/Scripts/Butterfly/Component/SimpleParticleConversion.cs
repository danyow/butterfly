namespace Butterfly.Component
{
    namespace Butterfly.Component
    {
        public class SimpleParticleConversion: GameObjectConversionSystem
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
}