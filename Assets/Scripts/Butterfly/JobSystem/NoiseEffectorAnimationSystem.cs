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
            var dt = Time.DeltaTime;
            Entities
               .ForEach(
                    (ref Unity.Transforms.Translation translation, in Component.NoiseEffector effector) =>
                    {
                        translation = new Unity.Transforms.Translation
                        {
                            Value = new Unity.Mathematics.float3(translation.Value.x, translation.Value.y - dt * effector.animationSpeed, translation.Value.z)
                        };
                    }
                )
               .ScheduleParallel();
        }
    }
}