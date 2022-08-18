namespace Butterfly.Component
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Noise Effector")]
    public class NoiseEffectorAuthoring: UnityEngine.MonoBehaviour
    {
        /// <summary>
        /// 频率
        /// </summary>
        public float frequency;

        /// <summary>
        /// 振幅
        /// </summary>
        public float amplitude;

        private void OnDrawGizmos()
        {
            UnityEngine.Gizmos.matrix = transform.localToWorldMatrix;
            UnityEngine.Gizmos.color = new UnityEngine.Color(1, 1, 0, 0.5f);
            UnityEngine.Gizmos.DrawWireCube(UnityEngine.Vector3.zero, UnityEngine.Vector3.one);
        }
    }
}