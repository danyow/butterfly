namespace Butterfly.Component.Particles
{
    [UnityEngine.AddComponentMenu("Butterfly/Wave Particle")]
    internal sealed class WaveParticleAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("权重")]
        public float weight = 1;

        [UnityEngine.HeaderAttribute("存活时间")]
        public float life = 999;
    }
}