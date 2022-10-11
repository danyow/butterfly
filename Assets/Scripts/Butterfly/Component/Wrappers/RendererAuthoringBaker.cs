using Unity.Entities;

namespace Butterfly.Component.Wrappers
{
    [UpdateAfter(typeof(RenderSettingsAuthoringBaker))]
    internal sealed class RendererAuthoringBaker: Baker<RendererAuthoring>
    {
        // protected override void OnUpdate()
        // {
        //     Entities.ForEach(
        //         (RendererAuthoring authoring) =>
        //         {
        //             var entity = GetPrimaryEntity(authoring);
        //             DstEntityManager.AddSharedComponentData(
        //                 entity,
        //                 new Renderer { workMesh = authoring.workMesh, }
        //             );
        //         }
        //     );
        // }

        public override void Bake(RendererAuthoring authoring)
        {
            AddSharedComponentManaged(new Renderer { workMesh = authoring.workMesh, });
        }
    }
}