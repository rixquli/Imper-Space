public class Wall : BuildingsBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        canBeRotated = true;
    }
}
