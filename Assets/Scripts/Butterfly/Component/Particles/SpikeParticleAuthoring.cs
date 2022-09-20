namespace Butterfly.Component.Particles
{
    [UnityEngine.AddComponentMenu("Butterfly/Spike Particle")]
    internal sealed class SpikeParticleAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("权重")]
        public float weight = 1;

        [UnityEngine.HeaderAttribute("存活时间")]
        public float life = 999;
    }
}