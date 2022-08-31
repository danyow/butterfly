using Unity.Entities;
using Butterfly.Component;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantExtendsListEntry
namespace Butterfly
{
    internal sealed partial class ParticleExpirationSystem: SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
               .ForEach(
                    (Entity entity, in Particle particle) =>
                    {
                        if(particle.time > 3 + particle.random * 5)
                        {
                            EntityManager.DestroyEntity(entity);
                        }
                    }
                )
               .WithStructuralChanges()
               .Run();
        }
    }
}