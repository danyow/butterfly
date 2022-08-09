// using System.Collections.Generic;
// using System.Linq;
// using Unity.Entities;
//
// [UpdateAfter(typeof(FlyAnimationSystem))]
// public class FlyRendererSystem : ComponentSystem
// {
//     private List<FlyRenderer> _sharedDataCache;
//
//     private FlyAnimationSystem _flyAnimationSystem;
//
//     private FlyRenderer? GetSharedData()
//     {
//         _sharedDataCache.Clear();
//         EntityManager.GetAllUniqueSharedComponentData(_sharedDataCache);
//         foreach (var data in _sharedDataCache.Where(data => data.material != null))
//         {
//             return data;
//         }
//         return null;
//     }
//
//     protected override void OnCreate()
//     {
//         base.OnCreate();
//         _flyAnimationSystem = World.GetOrCreateSystem<FlyAnimationSystem>();
//         _sharedDataCache = new List<FlyRenderer>(10);
//     }
//
//     protected override void OnUpdate()
//     {
//         var sharedData = GetSharedData();
//         if (sharedData == null)
//         {
//             return;
//         }
//         UnityEngine.Graphics.DrawMesh(_flyAnimationSystem.SharedMesh, UnityEngine.Matrix4x4.identity, sharedData.Value.material, 0);
//     }
// }