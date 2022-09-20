namespace Butterfly.Component.Wrappers
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Render Settings")]
    internal sealed class RenderSettingsAuthoring: UnityEngine.MonoBehaviour
    {
        [UnityEngine.HeaderAttribute("投射阴影模式")]
        public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.Off;

        [UnityEngine.HeaderAttribute("是否接受阴影")]
        public bool receiveShadows = true;

        [UnityEngine.HeaderAttribute("材质")]
        public UnityEngine.Material material;
    }
}