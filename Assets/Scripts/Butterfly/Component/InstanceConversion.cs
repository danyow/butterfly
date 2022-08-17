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
                        new Instance { templateMesh = authoring.templateMesh, }
                    );

                    // 加入缩放 否者为1的缩放无法获取
                    DstEntityManager.AddComponentData(
                        entity,
                        new Unity.Transforms.NonUniformScale { Value = authoring.transform.lossyScale, }
                    );
                }
            );
        }
    }
}