using System.Collections.Generic;
using Butterfly.Component;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

// ReSharper disable NotAccessedField.Local
namespace Butterfly
{
    [UpdateAfter(typeof(DisintegratorReconstructionSystem))]
    public class FlyRendererSystem: ComponentSystem
    {
        private readonly List<Renderer> _renderers = new List<Renderer>();
        private EntityQuery _dependency; // 仅用于依赖跟踪

        private UnityEngine.Vector3[] _managedVertexArray;
        private UnityEngine.Vector3[] _managedNormalArray;
        private int[] _managedIndexArray;

        protected override void OnCreate()
        {
            _dependency = GetEntityQuery(typeof(Disintegrator), typeof(Renderer));
            _managedVertexArray = new UnityEngine.Vector3[Renderer.kMaxVertices];
            _managedNormalArray = new UnityEngine.Vector3[Renderer.kMaxVertices];
            _managedIndexArray = new int[Renderer.kMaxVertices];

            for(var i = 0; i < Renderer.kMaxVertices; i++)
            {
                _managedIndexArray[i] = i;
            }
        }

        protected override void OnDestroy()
        {
            _managedIndexArray = null;
            _managedNormalArray = null;
            _managedVertexArray = null;
        }

        protected override unsafe void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(_renderers);

            var matrix = UnityEngine.Matrix4x4.identity;
            var copySize = sizeof(float3) * Renderer.kMaxVertices;

            var pVArray = UnsafeUtility.AddressOf(ref _managedVertexArray[0]);
            var pNArray = UnsafeUtility.AddressOf(ref _managedNormalArray[0]);

            foreach(var renderer in _renderers)
            {
                if(renderer.workMesh == null)
                {
                    continue;
                }

                var meshIsReady = renderer.workMesh.vertexCount > 0;

                if(!meshIsReady)
                {
                    renderer.workMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }

                UnsafeUtility.MemCpy(pVArray, renderer.vertices.GetUnsafePtr(), copySize);
                UnsafeUtility.MemCpy(pNArray, renderer.normals.GetUnsafePtr(), copySize);

                renderer.workMesh.vertices = _managedVertexArray;
                renderer.workMesh.normals = _managedNormalArray;
                if(!meshIsReady)
                {
                    renderer.workMesh.triangles = _managedIndexArray;
                }

                UnityEngine.Graphics.DrawMesh(renderer.workMesh, matrix, renderer.settings.material, 0);
            }

            _renderers.Clear();
        }
    }
}