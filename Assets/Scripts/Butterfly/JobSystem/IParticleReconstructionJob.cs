using Butterfly.Component;
using Unity.Collections;
using Unity.Transforms;

namespace Butterfly.JobSystem
{
    public interface IParticleReconstructionJob
    {
        NativeArray<Particle> GetParticles();
        NativeArray<Triangle> GetTriangles();
        NativeArray<Translation> GetTranslations();

        void Initialize(
            Unity.Entities.EntityQuery query,
            UnityEngine.Vector3[] vertices,
            UnityEngine.Vector3[] normals,
            Butterfly.Utility.NativeCounter.Concurrent counter
        );
    }
}