namespace Butterfly.Component.Wrappers
{
    internal sealed class NoiseEffectorConversionBaker: Unity.Entities.Baker<NoiseEffectorAuthoring>
    {
        // protected override void OnUpdate()
        // {
        //     Entities.ForEach(
        //         (NoiseEffectorAuthoring authoring) =>
        //         {
        //             var entity = GetPrimaryEntity(authoring);
        //             DstEntityManager.AddComponentData(
        //                 entity,
        //                 new NoiseEffector
        //                 {
        //                     frequency = authoring.frequency, amplitude = authoring.amplitude, animationSpeed = authoring.animationSpeed,
        //                 }
        //             );
        //             DstEntityManager.AddComponent<Unity.Transforms.WorldToLocal>(entity);
        //         }
        //     );
        // }

        public override void Bake(NoiseEffectorAuthoring authoring)
        {
            AddComponent(
                new NoiseEffector { frequency = authoring.frequency, amplitude = authoring.amplitude, animationSpeed = authoring.animationSpeed, }
            );
            AddComponent(new Unity.Transforms.ParentToWorldTransform());
        }
    }
}