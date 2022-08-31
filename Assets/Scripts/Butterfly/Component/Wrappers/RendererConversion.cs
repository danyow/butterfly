using Unity.Entities;

namespace Butterfly.Component.Wrappers
{
    [UpdateAfter(typeof(RenderSettingsConversion))]
    internal sealed class RendererConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (RendererAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new Renderer { workMesh = authoring.workMesh, }
                    );
                }
            );
        }
    }
}