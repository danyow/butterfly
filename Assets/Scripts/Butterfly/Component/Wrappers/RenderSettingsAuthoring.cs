namespace Butterfly.Component.Wrappers
{
    [UnityEngine.AddComponentMenu("Butterfly/Butterfly Render Settings")]
    internal sealed class RenderSettingsAuthoring: UnityEngine.MonoBehaviour
    {
        public UnityEngine.Material material;
        public UnityEngine.Rendering.ShadowCastingMode castShadows;
        public bool receiveShadows;
    }
}