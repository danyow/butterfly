namespace Butterfly.Component.Particles
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Particle")]
    internal sealed class ButterflyParticleAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("权重")]
        public float weight = 1;

        [UnityEngine.HeaderAttribute("存活时间")]
        public float life = 6;

        [UnityEngine.HeaderAttribute("尺寸")]
        public float size = 0.015f;
    }
}