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
            Entities
               .ForEach(
                    (ref Disintegrator disintegrator, ref Translation translation) =>
                    {
                        var np = translation.Value * 2;

                        noise.snoise(np, out var grad1);
                        noise.snoise(np + 10, out var grad2);

                        var acc = math.cross(grad1, grad2) * 0.02f;

                        translation.Value += disintegrator.velocity * dt;
                        disintegrator.life += dt;
                        disintegrator.velocity += acc * dt;
                    }
                )
               .ScheduleParallel();
        }
    }
}