namespace Butterfly.Component
{
    [UnityEngine.AddComponentMenu("Butterfly Render Settings")]
    public class RenderSettingsAuthoring: UnityEngine.MonoBehaviour
    {
        public UnityEngine.Material material;
        public UnityEngine.Rendering.ShadowCastingMode castShadows;
        public bool receiveShadows;
    }
}