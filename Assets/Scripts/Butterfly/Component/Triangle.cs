using Unity.Entities;
using Unity.Mathematics;

namespace Butterfly.Component
{
    /// <summary>
    /// 三角形
    /// </summary>
    public struct Triangle: IComponentData
    {
        public float3 vertex1;
        public float3 vertex2;
        public float3 vertex3;
    }
}