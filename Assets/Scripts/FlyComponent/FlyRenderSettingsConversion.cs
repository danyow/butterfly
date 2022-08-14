namespace FlyComponent
{
    public class FlyRenderSettingsConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (FlyRenderSettingsAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new FlyRenderSettings
                        {
                            material = authoring.material, castingMode = authoring.castingMode, receiveShadows = authoring.receiveShadows,
                        }
                    );
                }
            );
        }
    }
}