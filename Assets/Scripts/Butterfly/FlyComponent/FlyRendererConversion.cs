using Unity.Entities;

namespace Butterfly.FlyComponent
{
    [UpdateAfter(typeof(FlyRenderSettingsConversion))]
    public class FlyRendererConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (FlyRendererAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddSharedComponentData(
                        entity,
                        new FlyRenderer { meshInstance = authoring.meshInstance, }
                    );
                }
            );
        }
    }
}