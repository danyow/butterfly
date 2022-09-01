namespace Butterfly.Component.Wrappers
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Render Settings")]
    internal sealed class RenderSettingsAuthoring: UnityEngine.MonoBehaviour
    {
        public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.Off;
        public bool receiveShadows = true;
        public UnityEngine.Material material;
    }
}