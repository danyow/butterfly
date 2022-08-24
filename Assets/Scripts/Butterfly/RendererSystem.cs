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

        private readonly int[] _indexArray = new int[Renderer.MaxVertices];

        protected override void OnCreate()
        {
            _dependency = GetEntityQuery(typeof(Particle), typeof(Renderer));

            // 默认索引数组
            for(var i = 0; i < Renderer.MaxVertices; i++)
            {
                _indexArray[i] = i;
            }
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

                // 清除顶点缓冲区中未使用的部分。
                var vertexCount = renderer.counter.count * 3;

                UnsafeUtility.MemClear(
                    UnsafeUtility.AddressOf(ref renderer.vertices[vertexCount]),
                    sizeof(float3) * (Renderer.MaxVertices - vertexCount)
                );

                // 通过托管缓冲区更新顶点/法线数组。
                mesh.vertices = renderer.vertices;
                mesh.normals = renderer.normals;

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