namespace Butterfly
{
    public interface IParticleReconstructionJob
    {
        void Initialize(
            Unity.Entities.EntityQuery query,
            UnityEngine.Vector3[] vertices,
            UnityEngine.Vector3[] normals,
            Butterfly.Utility.NativeCounter.Concurrent counter
        );
    }
}