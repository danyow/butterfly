namespace Butterfly.Component
{
    internal sealed class RenderSettingsConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (RenderSettingsAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new RenderSettings
                        {
                            material = authoring.material, castShadows = authoring.castShadows, receiveShadows = authoring.receiveShadows,
                        }
                    );
                }
            );
        }
    }
}