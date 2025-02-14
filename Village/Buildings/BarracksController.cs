public class BarracksController : BuildingsBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        canOpenRecruitPanel = true;
    }
}
