using System.Collections.Generic;
using Butterfly.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantExtendsListEntry
namespace Butterfly.JobSystem
{
    public partial class ParticleExpirationSystemBase<T>: SystemBase
        where T: struct, ISharedComponentData, Butterfly.Component.Interface.IParticleVariant
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
                    if(particles[i].time > life * particles[i].lifeRandom)
                    {
                        ecb.DestroyEntity(entities[i]);
                    }
                }
            }
        }

        private readonly List<T> _variants = new List<T>();

        private EntityQuery _query;

        private BeginSimulationEntityCommandBufferSystem _simEcbSystem;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(typeof(Particle), typeof(T));
            _simEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _simEcbSystem.CreateCommandBuffer();

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
                _simEcbSystem.AddJobHandleForProducer(Dependency);
            }
            _variants.Clear();
        }
    }
}