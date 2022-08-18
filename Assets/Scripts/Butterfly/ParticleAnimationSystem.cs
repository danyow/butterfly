// using Butterfly.Component;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
//
// namespace Butterfly
// {
//     [UpdateBefore(typeof(ParticleReconstructionSystem))]
//     public partial class ParticleAnimationSystem: SystemBase
//     {
//         protected override void OnUpdate()
//         {
//             var time = (float)Time.ElapsedTime;
//             var deltaTime = Time.DeltaTime;
//             Entities
//                .ForEach(
//                     (ref Particle disintegrator, ref Translation translation) =>
//                     {
//                         var np = translation.Value * 6;
//
//                         noise.snoise(np, out var grad1);
//                         noise.snoise(np + 100, out var grad2);
//
//                         var acc = math.cross(grad1, grad2) * 0.02f;
//
//                         var dt = deltaTime * math.saturate(time - 2 + translation.Value.y * 2);
//
//                         translation.Value += disintegrator.velocity * dt;
//                         disintegrator.life += dt;
//                         disintegrator.velocity += acc * dt;
//                     }
//                 )
//                .ScheduleParallel();
//         }
//     }
// }