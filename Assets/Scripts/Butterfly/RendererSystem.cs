using System.Collections.Generic;
using Butterfly.Component;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

// ReSharper disable NotAccessedField.Local
namespace Butterfly
{
    [UpdateAfter(typeof(ParticleReconstructionSystem))]
    public class FlyRendererSystem: ComponentSystem
    {
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private EntityQuery _dependency; // 仅用于启用依赖项跟踪

        // 用于将数据注入网格的托管数组
        private UnityEngine.Vector3[] _vertexArray;
        private UnityEngine.Vector3[] _normalArray;
        private int[] _indexArray;

        protected override void OnCreate()
        {
            _dependency = GetEntityQuery(typeof(Particle), typeof(Renderer));

            // 分配临时托管数组。
            _vertexArray = new UnityEngine.Vector3[Renderer.MaxVertices];
            _normalArray = new UnityEngine.Vector3[Renderer.MaxVertices];
            _indexArray = new int[Renderer.MaxVertices];

            // 默认索引数组
            for(var i = 0; i < Renderer.MaxVertices; i++)
            {
                _indexArray[i] = i;
            }
        }

        protected override void OnDestroy()
        {
            _indexArray = null;
            _normalArray = null;
            _vertexArray = null;
        }

        protected override unsafe void OnUpdate()
        {
            var identityMatrix = UnityEngine.Matrix4x4.identity;

            // 迭代渲染器组件。
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            foreach(var renderer in _renderers)
            {
                var mesh = renderer.workMesh;

                // 如果没有网格（== 默认空数据），则不执行任何操作
                if(mesh == null)
                {
                    continue;
                }

                // 检查网格是否已被使用。
                var meshIsReady = mesh.vertexCount > 0;

                if(!meshIsReady)
                {
                    // Mesh初始设置：32位索引，动态更新
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                    // 优化网格以进行频繁更新。
                    mesh.MarkDynamic();
                }

                // 将顶点/法线数据复制到托管缓冲区中。
                var vertexCount = renderer.counter.count * 3;

                UnsafeUtility.MemCpy(
                    UnsafeUtility.AddressOf(ref _vertexArray[0]),
                    renderer.vertices.GetUnsafePtr(),
                    sizeof(float3) * vertexCount
                );

                UnsafeUtility.MemCpy(
                    UnsafeUtility.AddressOf(ref _normalArray[0]),
                    renderer.normals.GetUnsafePtr(),
                    sizeof(float3) * vertexCount
                );

                // 清除剩余的托管顶点缓冲区。
                UnsafeUtility.MemClear(
                    UnsafeUtility.AddressOf(ref _vertexArray[vertexCount]),
                    sizeof(float3) * (Renderer.MaxVertices - vertexCount)
                );

                // 通过托管缓冲区更新顶点/法线数组。
                mesh.vertices = _vertexArray;
                mesh.normals = _normalArray;

                if(!meshIsReady)
                {
                    // 首次设置默认索引数组。
                    mesh.triangles = _indexArray;

                    // 设置一个大的边界框以避免被剔除。
                    mesh.bounds = new UnityEngine.Bounds(UnityEngine.Vector3.zero, UnityEngine.Vector3.one * 1000);
                }

                // 绘制调用
                UnityEngine.Graphics.DrawMesh(mesh, identityMatrix, renderer.settings.material, 0);
            }

            _renderers.Clear();
        }
    }
}