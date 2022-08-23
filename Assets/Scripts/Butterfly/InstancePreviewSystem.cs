#if !UNITY_DISABLE_MANAGED_COMPONENTS

using Butterfly.Component;
using Unity.Entities;
using Unity.Transforms;
using RenderSettings = Butterfly.Component.RenderSettings;

namespace Butterfly
{
    // ReSharper disable once PartialTypeWithSinglePart
    [UnityEngine.ExecuteAlways]
    [AlwaysUpdateSystem]
    // [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class InstancePreviewSystem: ComponentSystem
    {
        private EntityQuery _query;

        protected override void OnCreate()
        {
            _query = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<Instance>(),
                        ComponentType.ReadOnly<RenderSettings>(),
                        ComponentType.ReadOnly<LocalToWorld>(),
                    },
                }
            );
        }

        protected override void OnUpdate()
        {
            var entities = _query.ToEntityArray(Unity.Collections.Allocator.Temp);
            for(var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var instance = EntityManager.GetSharedComponentData<Instance>(entity);
                var ltw = EntityManager.GetComponentData<LocalToWorld>(entity);
                var settings = EntityManager.GetSharedComponentData<RenderSettings>(entity);

                UnityEngine.Graphics.DrawMesh(instance.templateMesh, ltw.Value, settings.material, 0);
            }
        }
    }
}

#endif