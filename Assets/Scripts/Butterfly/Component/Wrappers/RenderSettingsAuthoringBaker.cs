namespace Butterfly.Component.Wrappers
{
    internal sealed class RenderSettingsAuthoringBaker: Unity.Entities.Baker<RenderSettingsAuthoring>
    {
        // protected override void OnUpdate()
        // {
        //     Entities.ForEach(
        //         (RenderSettingsAuthoring authoring) =>
        //         {
        //             var entity = GetPrimaryEntity(authoring);
        //             DstEntityManager.AddSharedComponentData(
        //                 entity,
        //                 new RenderSettings
        //                 {
        //                     material = authoring.material, castShadows = authoring.castShadows, receiveShadows = authoring.receiveShadows,
        //                 }
        //             );
        //         }
        //     );
        // }

        public override void Bake(RenderSettingsAuthoring authoring)
        {
            AddSharedComponentManaged(
                new RenderSettings
                {
                    material = authoring.material, castShadows = authoring.castShadows, receiveShadows = authoring.receiveShadows,
                }
            );
        }
    }
}