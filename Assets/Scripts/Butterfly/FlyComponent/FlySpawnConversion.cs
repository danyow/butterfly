namespace Butterfly.FlyComponent
{
    public class FlySpawnConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (FlySpawnAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new FlySpawn { templateMesh = authoring.templateMesh }
                    );
                }
            );
        }
    }
}