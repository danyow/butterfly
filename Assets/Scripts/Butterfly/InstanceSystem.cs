using System.Collections.Generic;
using Butterfly.Component;
using Butterfly.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Butterfly
{
    public class InstanceSystem: ComponentSystem
    {
        // 用于枚举实例组件
        private readonly List<Instance> _instanceDataList = new List<Instance>();
        private EntityQuery _instanceQuery;

        // 用于实例化的实体原型
        private EntityArchetype _archetype;

        // Allocation 跟踪
        private readonly List<Renderer> _toBeDisposed = new List<Renderer>();

        protected override void OnCreate()
        {
            _instanceQuery = GetEntityQuery(
                typeof(LocalToWorld),
                typeof(NonUniformScale),
                typeof(Instance),
                typeof(RenderSettings)
            );
            _archetype = EntityManager.CreateArchetype(
                typeof(Disintegrator),
                typeof(Facet),
                typeof(Translation),
                typeof(Renderer)
            );
        }

        protected override void OnDestroy()
        {
            foreach(var renderer in _toBeDisposed)
            {
                renderer.vertices.Dispose();
                renderer.normals.Dispose();
                renderer.counter.Dispose();
            }
        }

        protected override void OnUpdate()
        {
            // 枚举所有实例数据条目。
            EntityManager.GetAllUniqueSharedComponentData(_instanceDataList);

            foreach(var instanceData in _instanceDataList)
            {
                // 如果没有数据则跳过。
                if(!instanceData.templateMesh)
                {
                    continue;
                }

                // 获取实体数组的副本。不要直接使用迭代器——我们要移除缓冲区组件，它会使迭代器失效。
                _instanceQuery.SetSharedComponentFilter(instanceData);
                var iterator = _instanceQuery.ToEntityArray(Allocator.Temp);
                if(iterator.Length == 0)
                {
                    continue;
                }
                var instanceEntities = new NativeArray<Entity>(iterator.Length, Allocator.Temp);
                iterator.CopyTo(instanceEntities);

                // 检索网格数据。
                var vertices = instanceData.templateMesh.vertices;
                var indices = instanceData.templateMesh.triangles;

                // 实例化蝴蝶以及生成器实体。
                foreach(var instanceEntity in instanceEntities)
                {
                    // 检索源数据。
                    // var ltw = EntityManager.GetComponentData<LocalToWorld>(instanceEntity).Value;

                    // 为这个组创建一个渲染器。
                    var renderer = new Renderer
                    {
                        settings = EntityManager.GetSharedComponentData<RenderSettings>(instanceEntity),
                        vertices = new NativeArray<float3>(Renderer.kMaxVertices, Allocator.Persistent),
                        normals = new NativeArray<float3>(Renderer.kMaxVertices, Allocator.Persistent),
                        workMesh = new UnityEngine.Mesh(),
                        counter = new NativeCounter(Allocator.Persistent),
                    };

                    _toBeDisposed.Add(renderer);

                    var ltw = EntityManager.GetComponentData<LocalToWorld>(instanceEntity);
                    var scale = EntityManager.GetComponentData<NonUniformScale>(instanceEntity);
                    var matrix = float4x4.TRS(ltw.Position, ltw.Rotation, scale.Value);

                    // 填充实体。
                    for(var vi = 0; vi < indices.Length; vi += 3)
                    {
                        var v1 = math.mul(matrix, new float4(vertices[indices[vi + 0]], 1)).xyz;
                        var v2 = math.mul(matrix, new float4(vertices[indices[vi + 1]], 1)).xyz;
                        var v3 = math.mul(matrix, new float4(vertices[indices[vi + 2]], 1)).xyz;
                        var vc = (v1 + v2 + v3) / 3;

                        var entity = EntityManager.CreateEntity(_archetype);

                        EntityManager.SetComponentData(
                            entity,
                            new Facet { vertex1 = v1 - vc, vertex2 = v2 - vc, vertex3 = v3 - vc, }
                        );

                        EntityManager.SetComponentData(entity, new Translation { Value = vc, });

                        EntityManager.SetSharedComponentData(entity, renderer);
                    }

                    // 从实体中移除实例组件。
                    EntityManager.RemoveComponent(instanceEntity, typeof(Instance));
                }
                instanceEntities.Dispose();
            }
            _instanceDataList.Clear();
        }
    }
}