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
            var matrix = UnityEngine.Matrix4x4.identity;
            var copySize = sizeof(float3) * Renderer.MaxVertices;

            // 指向临时托管数组的指针
            var pVArray = UnsafeUtility.AddressOf(ref _vertexArray[0]);
            var pNArray = UnsafeUtility.AddressOf(ref _normalArray[0]);

            // 迭代渲染器组件。
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            foreach(var renderer in _renderers)
            {
                // 如果没有网格（== 默认空数据），则不执行任何操作
                if(renderer.workMesh == null)
                {
                    continue;
                }

                // 检查网格是否已被使用。
                var meshIsReady = renderer.workMesh.vertexCount > 0;

                if(!meshIsReady)
                {
                    // Mesh初始设置：32位索引，动态更新
                    renderer.workMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                    // 优化网格以进行频繁更新。
                    renderer.workMesh.MarkDynamic();
                }

                // 通过托管数组更新顶点/法线数组。
                UnsafeUtility.MemCpy(pVArray, renderer.vertices.GetUnsafePtr(), copySize);
                UnsafeUtility.MemCpy(pNArray, renderer.normals.GetUnsafePtr(), copySize);
                renderer.workMesh.vertices = _vertexArray;
                renderer.workMesh.normals = _normalArray;

                if(!meshIsReady)
                {
                    // 首次设置默认索引数组。
                    renderer.workMesh.triangles = _indexArray;

                    // 设置一个大的边界框以避免被剔除。
                    renderer.workMesh.bounds = new UnityEngine.Bounds(UnityEngine.Vector3.zero, UnityEngine.Vector3.one * 1000);
                }

                // 绘制调用
                UnityEngine.Graphics.DrawMesh(renderer.workMesh, matrix, renderer.settings.material, 0);
            }

            _renderers.Clear();
        }
    }
}