using Unity.Collections;
using Unity.Mathematics;

namespace Butterfly.FlyComponent
{
    public class FlyRendererAuthoring: UnityEngine.MonoBehaviour
    {
        public FlyRenderSettings settings;
        public NativeArray<float3> vertices;
        public NativeArray<float3> normals;
        public UnityEngine.Mesh meshInstance;
    }
}