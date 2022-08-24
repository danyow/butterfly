using Unity.Entities;
using Butterfly.Component;

// ReSharper disable RedundantExtendsListEntry
namespace Butterfly
{
    public partial class ParticleExpirationSystem: SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
               .ForEach(
                    (Entity entity, in Particle particle) =>
                    {
                        if(particle.life > 3 + particle.random * 5)
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