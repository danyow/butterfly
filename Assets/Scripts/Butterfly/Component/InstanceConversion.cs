namespace Butterfly.Component
{
    public class InstanceConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (InstanceAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new Instance { templateMesh = authoring.templateMesh }
                    );
                }
            );
        }
    }
}