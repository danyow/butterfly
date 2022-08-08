using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class FlyRendererSystem : ComponentSystem
{
    private List<FlyRenderer> _sharedDataCache;

    private FlySystem _flySystem;


    private FlyRenderer? GetSharedData()
    {
        _sharedDataCache.Clear();
        EntityManager.GetAllUniqueSharedComponentData(_sharedDataCache);
        foreach (var data in _sharedDataCache.Where(data => data.material != null))
        {
            return data;
        }
        return null;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        _flySystem = World.GetOrCreateSystem<FlySystem>();
        _sharedDataCache = new List<FlyRenderer>(10);
    }

    protected override void OnUpdate()
    {
        var sharedData = GetSharedData();
        if (sharedData == null)
        {
            return;
        }
        Graphics.DrawMesh(_flySystem.SharedMesh, Matrix4x4.identity, sharedData.Value.material, 0);
    }
}