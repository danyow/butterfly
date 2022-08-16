using Unity.Entities;

namespace Butterfly.Component
{
    [UpdateAfter(typeof(RenderSettingsConversion))]
    public class RendererConversion: GameObjectConversionSystem
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