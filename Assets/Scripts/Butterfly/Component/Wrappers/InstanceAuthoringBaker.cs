namespace Butterfly.Component.Wrappers
{
    internal sealed class InstanceAuthoringBaker: Unity.Entities.Baker<InstanceAuthoring>
    {
        // protected override void OnUpdate()
        // {
        //     Entities.ForEach(
        //         (InstanceAuthoring authoring) =>
        //         {
        //             var entity = GetPrimaryEntity(authoring);
        //             DstEntityManager.AddSharedComponentData(
        //                 entity,
        //                 new Instance { templateMesh = authoring.templateMesh, effectRate = authoring.effectRate, }
        //             );
        //
        //             // 加入缩放 否者为1的缩放无法获取
        //             DstEntityManager.AddComponentData(
        //                 entity,
        //                 new Unity.Transforms.NonUniformScale { Value = authoring.transform.lossyScale, }
        //             );
        //
        //             DstEntityManager.AddComponent<Unity.Transforms.LocalToWorld>(entity);
        //         }
        //     );
        // }

        public override void Bake(InstanceAuthoring authoring)
        {
            AddSharedComponentManaged(new Instance { templateMesh = authoring.templateMesh, effectRate = authoring.effectRate, });

            // AddComponent(new Unity.Transforms.UniformScaleTransform {Scale = authoring.transform.lossyScale,});
        }
    }
}