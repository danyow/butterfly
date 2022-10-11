using Unity.Entities;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantExtendsListEntry
namespace Butterfly.JobSystem
{
    [UpdateBefore(typeof(NoiseEffectorSystem))]
    internal sealed partial class NoiseEffectorAnimationSystem: SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = World.Time.DeltaTime;
            Entities
               .ForEach(
                    (ref Unity.Transforms.LocalToWorldTransform transform, in Component.NoiseEffector effector) =>
                    {
                        transform = new Unity.Transforms.LocalToWorldTransform
                        {
                            Value = Unity.Transforms.UniformScaleTransform.FromPosition(
                                transform.Value.Position.x,
                                transform.Value.Position.y - dt * effector.animationSpeed,
                                transform.Value.Position.z
                            )
                        };
                    }
                )
               .ScheduleParallel();
        }
    }
}