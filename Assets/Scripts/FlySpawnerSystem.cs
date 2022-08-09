using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FlySpawnerSystem : ComponentSystem
{

    // 用于枚举生成器组件
    private readonly List<FlySpawner> _spawners = new List<FlySpawner>();

    private EntityQuery _query;

    // 用于实例化的飞行实体原型
    private EntityArchetype _flyArchetype;

    protected override void OnCreate()
    {
        base.OnCreate();
        _query = GetEntityQuery(
            typeof(FlySpawner),
            typeof(Translation),
            typeof(FlyRenderer)
        );
        _flyArchetype = EntityManager.CreateArchetype(
            typeof(Fly),
            typeof(Facet),
            typeof(Translation),
            typeof(FlyRenderer)
        );
    }

    protected override void OnUpdate()
    {
        // 枚举所有生成器。
        EntityManager.GetAllUniqueSharedComponentData(_spawners);

        foreach (var spawner in _spawners)
        {
            // 如果没有数据则跳过。
            if (!spawner.templateMesh)
            {
                continue;
            }


            // 获取实体数组的副本。不要直接使用迭代器——我们要移除缓冲区组件，它会使迭代器失效。
            _query.SetSharedComponentFilter(spawner);
            var iterator = _query.ToEntityArray(Allocator.Temp);
            if (iterator.Length == 0)
            {
                continue;
            }
            var entities = new NativeArray<Entity>(iterator.Length, Allocator.Temp);
            iterator.CopyTo(entities);

            // 检索网格数据。
            var vertices = spawner.templateMesh.vertices;
            var indices = spawner.templateMesh.triangles;

            // 实例化蝴蝶以及生成器实体。
            foreach (var entity in entities)
            {
                // 检索位置数据。
                var position = EntityManager.GetComponentData<Translation>(entity).Value;
                var renderer = EntityManager.GetSharedComponentData<FlyRenderer>(entity);

                for (var vi = 0; vi < indices.Length; vi += 3)
                {
                    var v1 = (float3)vertices[indices[vi + 0]];
                    var v2 = (float3)vertices[indices[vi + 1]];
                    var v3 = (float3)vertices[indices[vi + 2]];
                    var vc = (v1 + v2 + v3) / 3;

                    var fly = EntityManager.CreateEntity(_flyArchetype);

                    EntityManager.SetComponentData(
                        fly,
                        new Facet
                        {
                            vertex1 = v1 - vc,
                            vertex2 = v2 - vc,
                            vertex3 = v3 - vc
                        }
                    );

                    EntityManager.SetComponentData(fly, new Translation { Value = position + vc });

                    EntityManager.SetSharedComponentData(fly, renderer);
                }

                // 从实体中移除 spawner 组件。
                EntityManager.RemoveComponent(entity, typeof(FlySpawner));
            }
            entities.Dispose();
        }
        _spawners.Clear();
    }
}