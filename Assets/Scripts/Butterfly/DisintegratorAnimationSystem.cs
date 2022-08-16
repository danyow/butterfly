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
            var time = (float)Time.ElapsedTime;
            var deltaTime = Time.DeltaTime;
            UnityEngine.Debug.Log($"{time} ==> {deltaTime}");
            Entities
               .ForEach(
                    (ref Disintegrator disintegrator, ref Translation translation) =>
                    {
                        var np = translation.Value * 2;

                        noise.snoise(np, out var grad1);
                        noise.snoise(np + 100, out var grad2);

                        var acc = math.cross(grad1, grad2) * 0.02f;

                        var dt = deltaTime * math.saturate(time - 2 + translation.Value.y * 2);

                        translation.Value += disintegrator.velocity * dt;
                        disintegrator.life += dt;
                        disintegrator.velocity += acc * dt;
                    }
                )
               .ScheduleParallel();
        }
    }
}