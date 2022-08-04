using Unity.Entities;

public class FlySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Fly fly) => { fly.life += Time.DeltaTime; });
    }
    
}