using Unity.Entities;

namespace Butterfly
{
    [UpdateBefore(typeof(NoiseEffectorSystem))]
    // ReSharper disable once RedundantExtendsListEntry
    public partial class NoiseEffectorAnimationSystem: SystemBase
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
                            Value = new Unity.Mathematics.float3(translation.Value.x, translation.Value.y - dt * 0.1f, translation.Value.z)
                        };
                    }
                )
               .ScheduleParallel();
        }
    }
}