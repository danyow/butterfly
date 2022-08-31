using System.Collections.Generic;
using Butterfly.Component;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

// ReSharper disable PartialTypeWithSinglePart
namespace Butterfly.JobSystem
{
    public partial class ParticleReconstructionSystemBase<TVariant, TJob>: SystemBase
        where TVariant: struct, ISharedComponentData, Butterfly.Component.Interface.IParticleVariant
        where TJob: struct, IJobParallelFor, IParticleReconstructionJob<TVariant>
    {
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private readonly List<TVariant> _variants = new List<TVariant>();

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
            EntityManager.GetAllUniqueSharedComponentData(_variants);

            var job = new TJob();

            foreach(var renderer in _renderers)
            {
                foreach(var variant in _variants)
                {
                    _query.SetSharedComponentFilter(renderer, variant);

                    var count = _query.CalculateEntityCount();

                    if(count == 0)
                    {
                        continue;
                    }

                    job.Initialize(
                        variant,
                        _query,
                        renderer.vertices,
                        renderer.normals,
                        renderer.concurrentCounter
                    );

                    Dependency = job.Schedule(count, 8, Dependency);

                    Dependency = job.GetParticles().Dispose(Dependency);
                    Dependency = job.GetTriangles().Dispose(Dependency);
                    Dependency = job.GetTranslations().Dispose(Dependency);
                }
            }

            _renderers.Clear();
            _variants.Clear();
        }
    }
}