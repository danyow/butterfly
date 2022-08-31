using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Butterfly.Component;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantExtendsListEntry
namespace Butterfly
{
    public partial class ParticleExpirationSystem<T>: SystemBase where T: struct, ISharedComponentData, IParticleVariant
    {
        [BurstCompile]
        private struct ParticleExpirationJob: IJob
        {
            [ReadOnly]
            public NativeArray<Entity> entities;

            [ReadOnly]
            public NativeArray<Particle> particles;

            public float life;
            public EntityCommandBuffer ecb;

            public void Execute()
            {
                for(var i = 0; i < entities.Length; i++)
                {
                    var calLife = life * (particles[i].random + 1) * 0.5f;
                    if(particles[i].time > calLife)
                    {
                        ecb.DestroyEntity(entities[i]);
                    }
                }
            }
        }

        private readonly List<T> _variants = new List<T>();

        private EntityQuery _query;

        private BeginSimulationEntityCommandBufferSystem simEcbSystem;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(typeof(Particle), typeof(T));
            simEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = simEcbSystem.CreateCommandBuffer();

            EntityManager.GetAllUniqueSharedComponentData(_variants);

            foreach(var variant in _variants)
            {
                _query.SetSharedComponentFilter(variant);

                var job = new ParticleExpirationJob
                {
                    entities = _query.ToEntityArray(Allocator.TempJob),
                    particles = _query.ToComponentDataArray<Particle>(Allocator.TempJob),
                    life = variant.GetLife(),
                    ecb = ecb,
                };
                Dependency = job.Schedule(Dependency);

                Dependency = job.particles.Dispose(Dependency);
                Dependency = job.entities.Dispose(Dependency);
                
                // 为生产者添加作业句柄
                simEcbSystem.AddJobHandleForProducer(Dependency);
            }
            _variants.Clear();
        }
    }
}