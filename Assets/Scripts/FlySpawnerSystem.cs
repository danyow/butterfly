using System.Collections.Generic;
using FlyComponent;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FlySpawnerSystem: ComponentSystem
{
    // 用于枚举生成器组件
    private readonly List<FlySpawn> _spawnDatas = new List<FlySpawn>();
    private EntityQuery _spawnQuery;

    // 用于实例化的飞行实体原型
    private EntityArchetype _flyArchetype;

    private List<NativeArray<float3>> _toBeDisposed = new List<NativeArray<float3>>();

    protected override void OnCreate()
    {
        _spawnQuery = GetEntityQuery(
            typeof(Translation),
            typeof(FlySpawn),
            typeof(FlyRenderSettings)
        );
        _flyArchetype = EntityManager.CreateArchetype(
            typeof(Fly),
            typeof(Facet),
            typeof(Translation),
            typeof(FlyRenderer)
        );
    }

    protected override void OnDestroy()
    {
        for(var i = 0; i < _toBeDisposed.Count; i++)
        {
            _toBeDisposed[i].Dispose();
        }
    }

    protected override void OnUpdate()
    {
        // 枚举所有生成器。
        EntityManager.GetAllUniqueSharedComponentData(_spawnDatas);

        foreach(var spawner in _spawnDatas)
        {
            // 如果没有数据则跳过。
            if(!spawner.templateMesh)
            {
                continue;
            }

            // 获取实体数组的副本。不要直接使用迭代器——我们要移除缓冲区组件，它会使迭代器失效。
            _spawnQuery.SetSharedComponentFilter(spawner);
            var iterator = _spawnQuery.ToEntityArray(Allocator.Temp);
            if(iterator.Length == 0)
            {
                continue;
            }
            var spawnEntities = new NativeArray<Entity>(iterator.Length, Allocator.Temp);
            iterator.CopyTo(spawnEntities);

            // 检索网格数据。
            var vertices = spawner.templateMesh.vertices;
            var indices = spawner.templateMesh.triangles;

            // 实例化蝴蝶以及生成器实体。
            foreach(var spawnEntity in spawnEntities)
            {
                // 检索位置数据。
                var position = EntityManager.GetComponentData<Translation>(spawnEntity).Value;

                var renderer = new FlyRenderer
                {
                    settings = EntityManager.GetSharedComponentData<FlyRenderSettings>(spawnEntity),
                    vertices = new NativeArray<float3>(FlyRenderer.kMaxVertices, Allocator.Persistent),
                    normals = new NativeArray<float3>(FlyRenderer.kMaxVertices, Allocator.Persistent),
                    meshInstance = new UnityEngine.Mesh(),
                };

                _toBeDisposed.Add(renderer.vertices);
                _toBeDisposed.Add(renderer.normals);

                // 填充Fly实体
                for(var vi = 0; vi < indices.Length; vi += 3)
                {
                    var v1 = (float3)vertices[indices[vi + 0]];
                    var v2 = (float3)vertices[indices[vi + 1]];
                    var v3 = (float3)vertices[indices[vi + 2]];
                    var vc = (v1 + v2 + v3) / 3;

                    var fly = EntityManager.CreateEntity(_flyArchetype);

                    EntityManager.SetComponentData(
                        fly,
                        new Facet { vertex1 = v1 - vc, vertex2 = v2 - vc, vertex3 = v3 - vc, }
                    );

                    EntityManager.SetComponentData(fly, new Translation { Value = position + vc, });

                    EntityManager.SetSharedComponentData(fly, renderer);
                }

                // 从实体中移除 spawner 组件。
                EntityManager.RemoveComponent(spawnEntity, typeof(FlySpawn));
            }
            spawnEntities.Dispose();
        }
        _spawnDatas.Clear();
    }
}