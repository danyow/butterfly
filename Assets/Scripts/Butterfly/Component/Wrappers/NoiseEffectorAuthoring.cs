namespace Butterfly.Component.Wrappers
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Noise Effector")]
    internal sealed class NoiseEffectorAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("频率")]
        public float frequency = 6f;

        [UnityEngine.HeaderAttribute("振幅")]
        public float amplitude = 0.02f;

        [UnityEngine.HeaderAttribute("动画速度")]
        public float animationSpeed = 0.2f;

        private void OnDrawGizmos()
        {
            UnityEngine.Gizmos.matrix = transform.localToWorldMatrix;
            UnityEngine.Gizmos.color = new UnityEngine.Color(1, 1, 0, 0.5f);
            UnityEngine.Gizmos.DrawWireCube(UnityEngine.Vector3.zero, UnityEngine.Vector3.one);
        }
    }
}