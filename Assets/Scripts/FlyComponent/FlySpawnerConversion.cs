namespace FlyComponent
{
    public class FlySpawnerConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (FlySpawnerAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new FlySpawner { templateMesh = authoring.templateMesh }
                    );
                }
            );
        }
    }
}