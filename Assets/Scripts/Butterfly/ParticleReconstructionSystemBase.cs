using System.Collections.Generic;
using Butterfly.Component;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly
{
    public partial class ParticleReconstructionSystemBase<TVariant, TJob>: SystemBase
        where TVariant: struct, ISharedComponentData, IParticleVariant
        where TJob: struct, IJobParallelFor, IParticleReconstructionJob
    {
        private readonly List<Renderer> _renderers = new List<Renderer>();

        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                typeof(Renderer),
                typeof(TVariant),
                typeof(Particle),
                typeof(Translation),
                typeof(Triangle)
            );
        }

        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            var job = new TJob();

            foreach(var renderer in _renderers)
            {
                _query.SetSharedComponentFilter(renderer);

                var count = _query.CalculateEntityCount();

                if(count == 0)
                {
                    continue;
                }

                job.Initialize(
                    _query,
                    renderer.vertices,
                    renderer.normals,
                    renderer.concurrentCounter
                );

                Dependency = job.Schedule(count, 8, Dependency);
            }

            _renderers.Clear();
        }
    }
}