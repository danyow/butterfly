using Unity.Collections;
using Unity.Mathematics;

namespace Butterfly.Component
{
    public class RendererAuthoring: UnityEngine.MonoBehaviour
    {
        public RenderSettings settings;
        public NativeArray<float3> vertices;
        public NativeArray<float3> normals;
        public UnityEngine.Mesh workMesh;
    }
}