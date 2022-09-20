namespace Butterfly.Component.Particles
{
    [UnityEngine.AddComponentMenu("Butterfly/Simple Particle")]
    internal sealed class SimpleParticleAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("权重")]
        public float weight = 1;

        [UnityEngine.HeaderAttribute("存活时间")]
        public float life = 4;
    }
}