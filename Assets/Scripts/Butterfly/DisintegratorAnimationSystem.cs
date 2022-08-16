using Butterfly.Component;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Butterfly
{
    public partial class DisintegratorAnimationSystem: SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            Entities.ForEach(
                         (ref Disintegrator disintegrator, ref Translation translation) =>
                         {
                             noise.snoise(translation.Value, out var d1);
                             noise.snoise(translation.Value + 10, out var d2);

                             translation.Value += math.cross(d1, d2) * dt * 0.02f;
                             disintegrator.life += dt;
                         }
                     )
                    .ScheduleParallel();
        }
    }
}