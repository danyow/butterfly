namespace Butterfly.Component.Wrappers
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Instance")]
    internal sealed class InstanceAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("模板网格")]
        public UnityEngine.Mesh templateMesh;

        [UnityEngine.HeaderAttribute("特效率")]
        public float effectRate = 1;
    }
}