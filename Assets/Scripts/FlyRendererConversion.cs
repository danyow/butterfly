public class FlyRendererConversion: GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (FlyRendererAuthoring authoring) =>
            {
                var entity = GetPrimaryEntity(authoring);
                DstEntityManager.AddSharedComponentData(
                    entity,
                    new FlyRenderer
                    {
                        material = authoring.material, castingShadows = authoring.castShadows, receiveShadows = authoring.receiveShadows,
                    }
                );
            }
        );
    }
}