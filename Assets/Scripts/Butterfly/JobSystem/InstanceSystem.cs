#define DEBUG_DIAGNOSTICS

#if DEBUG_DIAGNOSTICS
using System.Diagnostics;
#endif
using System.Collections.Generic;
using Butterfly.Component;
using Butterfly.Component.Particles;
using Butterfly.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Butterfly.Utility.Random;
using Renderer = Butterfly.Component.Renderer;
using RenderSettings = Butterfly.Component.RenderSettings;

namespace Butterfly.JobSystem
{
    internal sealed class InstanceSystem: ComponentSystem
    {
#region 组件系统实现

        // 用于枚举实例组件
        private readonly List<Instance> _instanceDataList = new List<Instance>();
        private EntityQuery _instanceQuery;

        // 用于实例化的实体原型
        private EntityArchetype _archetype;

        // Allocation 跟踪
        private readonly List<Renderer> _toBeDisposed = new List<Renderer>();

        // 用于为粒子提供 id。
        private uint _indexCounter;

        protected override void OnCreate()
        {
            _instanceQuery = GetEntityQuery(
                typeof(LocalToWorld),
                typeof(NonUniformScale),
                typeof(Instance),
                typeof(RenderSettings)
            );
            _archetype = EntityManager.CreateArchetype(
                typeof(Particle),
                typeof(Triangle),
                typeof(Translation),
                typeof(Renderer)
            );
        }

        protected override void OnDestroy()
        {
            foreach(var renderer in _toBeDisposed)
            {
                // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
                Object.Destroy(renderer.workMesh);
                renderer.counter.Dispose();
            }
            _toBeDisposed.Clear();
        }

        protected override void OnUpdate()
        {
            // 在这个系统中有三个级别的循环：
            // 
            // 循环 1：通过唯一实例设置数组。我们会得到共享相同实例设置的实体数组。
            // 循环 2：通过循环 1 中的实体数组。
            // 循环 3：通过给定的模板网格中的顶点数组通过实例设置。

#if DEBUG_DIAGNOSTICS
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
#endif

            // 循环 1：迭代唯一的实例数据条目。
            EntityManager.GetAllUniqueSharedComponentData(_instanceDataList);

            foreach(var instanceData in _instanceDataList)
            {
                // 如果没有数据则跳过。
                if(instanceData.templateMesh == null)
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

                var instanceEntities = new NativeArray<Entity>(iterator.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                iterator.CopyTo(instanceEntities);

                // 检索网格数据。
                var vertices = instanceData.templateMesh.vertices;
                var indices = instanceData.templateMesh.triangles;

                // 循环 2：迭代实例实体。
                foreach(var instanceEntity in instanceEntities)
                {
                    // 检索源数据。
                    var ltw = EntityManager.GetComponentData<LocalToWorld>(instanceEntity);
                    var scale = EntityManager.GetComponentData<NonUniformScale>(instanceEntity);
                    var matrix = float4x4.TRS(ltw.Position, ltw.Rotation, scale.Value);

                    // 循环 3：迭代模板网格中的顶点。
                    CreateEntitiesOverMesh(
                        instanceEntity,
                        matrix,
                        vertices,
                        indices,
                        instanceData.effectRate
                    );

                    // 从实体中移除实例组件。
                    EntityManager.RemoveComponent(instanceEntity, typeof(Instance));
                }
                instanceEntities.Dispose();
            }
            _instanceDataList.Clear();
#if DEBUG_DIAGNOSTICS
            stopwatch.Stop();
            var time = 1000f * stopwatch.ElapsedTicks / Stopwatch.Frequency;
            UnityEngine.Debug.Log($"Instantiation time: {time}ms");
#endif
        }

#endregion

#region 默认实体表

        //该表用于创建具有加权的粒子实体粒子类型的随机分布。它存储选择权重和允许创建具有实例化的实体的默认实体。

        /// <summary>
        /// 默认实体条目
        /// </summary>
        private struct DefaultEntityEntry: System.IEquatable<DefaultEntityEntry>
        {
            public float weight;
            public Entity entity;

            public bool Equals(DefaultEntityEntry other)
            {
                return weight.Equals(other.weight) && entity.Equals(other.entity);
            }

            public override bool Equals(object obj)
            {
                return obj is DefaultEntityEntry other && Equals(other);
            }

            public override int GetHashCode()
            {
                return System.HashCode.Combine(weight, entity);
            }
        }

        private readonly DefaultEntityEntry[] _defaultEntityEntries = new DefaultEntityEntry[16];

        private void CreateDefaultEntity<T>(Entity sourceEntity, ref Renderer renderer)
            where T: struct, ISharedComponentData, Butterfly.Component.Interface.IParticleVariant
        {
            if(!EntityManager.HasComponent<T>(sourceEntity))
            {
                return;
            }

            // 变体
            var variant = EntityManager.GetSharedComponentData<T>(sourceEntity);
            var entity = EntityManager.CreateEntity(_archetype);
            EntityManager.SetSharedComponentData(entity, renderer);
            EntityManager.AddSharedComponentData(entity, variant);

            for(var i = 0; i < _defaultEntityEntries.Length; i++)
            {
                var defaultEntityEntry = _defaultEntityEntries[i];
                if(!defaultEntityEntry.Equals(default(DefaultEntityEntry)))
                {
                    continue;
                }
                _defaultEntityEntries[i] = new DefaultEntityEntry { weight = variant.GetWeight(), entity = entity, };
                break;
            }
        }

        /// <summary>
        /// 规范化默认实体权重
        /// </summary>
        private void NormalizeDefaultEntityWeights()
        {
            var total = 0f;

            for(var i = 0; i < _defaultEntityEntries.Length; i++)
            {
                total += _defaultEntityEntries[i].weight;
            }

            var subtotal = 0.0f;
            for(var i = 0; i < _defaultEntityEntries.Length; i++)
            {
                subtotal += _defaultEntityEntries[i].weight / total;
                _defaultEntityEntries[i].weight = subtotal;
            }
        }

        /// <summary>
        /// 选择随机默认实体
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        private Entity SelectRandomDefaultEntity(uint seed)
        {
            var rand = Random.Value01(seed);
            for(var i = 0; i < _defaultEntityEntries.Length; i++)
            {
                if(rand < _defaultEntityEntries[i].weight)
                {
                    return _defaultEntityEntries[i].entity;
                }
            }
            return Entity.Null;
        }

        /// <summary>
        /// 清理默认实体表
        /// </summary>
        private void CleanupDefaultEntityTable()
        {
            for(var i = 0; i < _defaultEntityEntries.Length; i++)
            {
                var entity = _defaultEntityEntries[i].entity;
                if(EntityManager.Exists(entity))
                {
                    EntityManager.DestroyEntity(entity);
                }
                _defaultEntityEntries[i] = default(DefaultEntityEntry);
            }
        }

#endregion

#region Jobified 初始化器

        // 我们使用并行作业来计算初始数据实例化实体中的组件。
        // 这样做的主要动机是用 Burst 优化向量数学运算
        // 我们不期望并行性会大大提高性能。
        [BurstCompile]
        private unsafe struct InitDataJob: IJobParallelFor
        {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public void* vertices;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public void* indices;

            [ReadOnly]
            public float4x4 ltw;

            public uint indexOffset;

            public float effectRate;

            public NativeArray<Triangle> triangles;
            public NativeArray<Particle> particles;
            public NativeArray<Translation> translations;

            public void Execute(int index)
            {
                var i1 = UnsafeUtility.ReadArrayElement<int>(indices, index * 3);
                var i2 = UnsafeUtility.ReadArrayElement<int>(indices, index * 3 + 1);
                var i3 = UnsafeUtility.ReadArrayElement<int>(indices, index * 3 + 2);

                var v1 = UnsafeUtility.ReadArrayElement<float3>(vertices, i1);
                var v2 = UnsafeUtility.ReadArrayElement<float3>(vertices, i2);
                var v3 = UnsafeUtility.ReadArrayElement<float3>(vertices, i3);

                v1 = math.mul(ltw, new float4(v1, 1)).xyz;
                v2 = math.mul(ltw, new float4(v2, 1)).xyz;
                v3 = math.mul(ltw, new float4(v3, 1)).xyz;

                var vc = (v1 + v2 + v3) / 3;

                triangles[index] = new Triangle { vertex1 = v1 - vc, vertex2 = v2 - vc, vertex3 = v3 - vc, };

                translations[index] = new Translation { Value = vc, };

                particles[index] = new Particle
                {
                    id = (uint)index + indexOffset, lifeRandom = Random.Value01((uint)index) * 0.8f + 0.2f, effectRate = effectRate,
                };
            }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        private unsafe void CreateEntitiesOverMesh(
            Entity sourceEntity,
            float4x4 matrix,
            Vector3[] vertices,
            int[] indices,
            float effectRate
        )
        {
            var entityCount = indices.Length / 3;

            // 使用并行作业计算初始数据。
            var job = new InitDataJob
            {
                vertices = UnsafeUtility.AddressOf(ref vertices[0]),
                indices = UnsafeUtility.AddressOf(ref indices[0]),
                ltw = matrix,
                indexOffset = _indexCounter,
                effectRate = effectRate,
                triangles = new NativeArray<Triangle>(entityCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                translations = new NativeArray<Translation>(entityCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                particles = new NativeArray<Particle>(entityCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
            };

            var jobHandle = job.Schedule(entityCount, 32);

            _indexCounter += (uint)entityCount;

            // 我们希望与作业并行进行实体实例化，所以让工作立即开始。
            JobHandle.ScheduleBatchedJobs();

            // 为这个查询创建一个渲染器。
            var counter = new NativeCounter(Allocator.Persistent);
            var renderer = new Renderer
            {
                settings = EntityManager.GetSharedComponentData<RenderSettings>(sourceEntity),
                workMesh = new Mesh(),
                vertices = new Vector3[Renderer.MaxVertices],
                normals = new Vector3[Renderer.MaxVertices],
                counter = counter,
                concurrentCounter = counter,
            };

            // 我们希望这个渲染器对象在世界的尽头被销毁。
            _toBeDisposed.Add(renderer);

            // 初始化默认实体表。
            CreateDefaultEntity<SimpleParticle>(sourceEntity, ref renderer);
            CreateDefaultEntity<ButterflyParticle>(sourceEntity, ref renderer);
            CreateDefaultEntity<SpikeParticle>(sourceEntity, ref renderer);
            CreateDefaultEntity<WaveParticle>(sourceEntity, ref renderer);
            NormalizeDefaultEntityWeights();

            // 创建一个克隆数组作为在每个三角形上放置一个克隆。
            var entities = new NativeArray<Entity>(entityCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for(var i = 0; i < entityCount; i++)
            {
                entities[i] = EntityManager.Instantiate(SelectRandomDefaultEntity((uint)i));
            }

            // 设置初始数据。
            jobHandle.Complete();

            for(var i = 0; i < entityCount; i++)
            {
                var entity = entities[i];

                EntityManager.SetComponentData(entity, job.triangles[i]);
                EntityManager.SetComponentData(entity, job.particles[i]);
                EntityManager.SetComponentData(entity, job.translations[i]);
            }

            // 销毁临时对象。
            entities.Dispose();

            CleanupDefaultEntityTable();

            job.triangles.Dispose();
            job.particles.Dispose();
            job.translations.Dispose();
        }

#endregion
    }
}