public class ShieldOnCardThreshold : OnCardThreshold
{
	public override bool OnEnable()
	{
		ApplyStatus(base.Owner, StatusEnum.Shield, base.Val2);
		return true;
	}
}
