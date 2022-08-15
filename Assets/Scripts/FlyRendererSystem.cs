using System.Collections.Generic;
using FlyComponent;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(FlyAnimationSystem))]
public class FlyRendererSystem: ComponentSystem
{
    private readonly List<FlyRenderer> _renderers = new List<FlyRenderer>();
    private EntityQuery _dependency; // 仅用于依赖跟踪

    private UnityEngine.Vector3[] _managedVertexArray;
    private UnityEngine.Vector3[] _managedNormalArray;
    private int[] _managedIndexArray;

    private int _testIndex;

    protected override void OnCreate()
    {
        _dependency = GetEntityQuery(typeof(Fly), typeof(FlyRenderer));
        _managedVertexArray = new UnityEngine.Vector3[FlyRenderer.kMaxVertices];
        _managedNormalArray = new UnityEngine.Vector3[FlyRenderer.kMaxVertices];
        _managedIndexArray = new int[FlyRenderer.kMaxVertices];

        for(var i = 0; i < FlyRenderer.kMaxVertices; i++)
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
        var copySize = sizeof(float3) * FlyRenderer.kMaxVertices;

        var pVArray = UnsafeUtility.AddressOf(ref _managedVertexArray[0]);
        var pNArray = UnsafeUtility.AddressOf(ref _managedNormalArray[0]);

        foreach(var renderer in _renderers)
        {
            if(renderer.meshInstance == null)
            {
                continue;
            }

            _testIndex++;
            if(math.ceil(_testIndex / 1000f) % 2 == 0)
            {
                if(renderer.settings.material.name == "Blue")
                {
                    continue;
                }
            }
            else
            {
                if(renderer.settings.material.name == "Red")
                {
                    continue;
                }
            }

            UnsafeUtility.MemCpy(pVArray, renderer.vertices.GetUnsafePtr(), copySize);
            UnsafeUtility.MemCpy(pNArray, renderer.normals.GetUnsafePtr(), copySize);

            renderer.meshInstance.vertices = _managedVertexArray;
            renderer.meshInstance.normals = _managedNormalArray;
            renderer.meshInstance.triangles = _managedIndexArray;

            UnityEngine.Graphics.DrawMesh(renderer.meshInstance, matrix, renderer.settings.material, 0);
        }

        _renderers.Clear();
    }
}