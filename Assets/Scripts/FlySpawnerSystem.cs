using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
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
        _query = GetEntityQuery(typeof(FlySpawner));
        _flyArchetype = EntityManager.CreateArchetype(typeof(Fly), typeof(Translation));
    }

    protected override void OnUpdate()
    {
        // 枚举所有生成器。
        EntityManager.GetAllUniqueSharedComponentData(_spawners);

        for (var i = 0; i < _spawners.Count; i++)
        {
            _query.SetSharedComponentFilter(_spawners[i]);

            // 获取实体数组的副本。不要直接使用迭代器——我们要移除缓冲区组件，它会使迭代器失效。
            var iterator = _query.ToEntityArray(Allocator.Temp);
            var entities = new NativeArray<Entity>(iterator.Length, Allocator.Temp);
            iterator.CopyTo(entities);

            // 实例化蝴蝶以及生成器实体。
            for (var j = 0; j < entities.Length; j++)
            {
                foreach (var v in _spawners[i].templateMesh.vertices)
                {
                    var fly = EntityManager.CreateEntity(_flyArchetype);
                    EntityManager.SetComponentData(fly, new Translation { Value = v });

                    // 从实体中移除 spawner 组件。
                    EntityManager.RemoveComponent(entities[j], typeof(FlySpawner));
                }
            }
            entities.Dispose();
        }
        _spawners.Clear();
    }
}