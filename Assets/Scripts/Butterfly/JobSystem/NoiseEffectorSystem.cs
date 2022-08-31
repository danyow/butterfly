using Butterfly.Component;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantExtendsListEntry
namespace Butterfly.JobSystem
{
    [UpdateBefore(typeof(SimpleParticleReconstructionSystem))]
    [UpdateBefore(typeof(ButterflyParticleReconstructionSystem))]
    internal sealed partial class NoiseEffectorSystem: SystemBase
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                new EntityQueryDesc { All = new[] { ComponentType.ReadOnly<NoiseEffector>(), ComponentType.ReadOnly<LocalToWorld>(), }, }
            );
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            var entities = _query.ToEntityArray(Allocator.Temp);

            for(var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var effector = EntityManager.GetComponentData<NoiseEffector>(entity);
                var wtl = EntityManager.GetComponentData<WorldToLocal>(entity);

                Entities
                   .ForEach(
                        (ref Particle particle, ref Translation translation) =>
                        {
                            var pos = translation.Value;
                            var acc = DFNoise(pos, effector) * effector.amplitude;

                            var dt = deltaTime * Amplitude(pos, wtl.Value);

                            particle.velocity += acc * dt;
                            particle.time += dt;
                            translation.Value += particle.velocity * dt;
                        }
                    )
                   .ScheduleParallel();
            }
            entities.Dispose();
        }

        private static float3 DFNoise(float3 p, [ReadOnly] NoiseEffector effector)
        {
            p *= effector.frequency;

            noise.snoise(p, out var grad1);

            p.z += 100;

            noise.snoise(p, out var grad2);

            return math.cross(grad1, grad2);
        }

        private static float Amplitude(float3 p, float4x4 transform)
        {
            var z = math.mul(transform, new float4(p, 1)).z;
            return math.saturate(z + 0.5f);
        }
    }
}