namespace Butterfly.Component
{
    internal sealed class NoiseEffectorConversion: GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (NoiseEffectorAuthoring authoring) =>
                {
                    var entity = GetPrimaryEntity(authoring);
                    DstEntityManager.AddComponentData(
                        entity,
                        new NoiseEffector { frequency = authoring.frequency, amplitude = authoring.amplitude, }
                    );
                    DstEntityManager.AddComponent<Unity.Transforms.WorldToLocal>(entity);
                }
            );
        }
    }
}