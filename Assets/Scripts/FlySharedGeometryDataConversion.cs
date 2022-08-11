public class FlySharedGeometryDataConversion: GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach(
            (FlySharedGeometryDataAuthoring authoring) =>
            {
                var entity = GetPrimaryEntity(authoring);
                DstEntityManager.AddSharedComponentData(
                    entity,
                    new FlySharedGeometryData { meshInstance = authoring.meshInstance, }
                );
            }
        );
    }
}