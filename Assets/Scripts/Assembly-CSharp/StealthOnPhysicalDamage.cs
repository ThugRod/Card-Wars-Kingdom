public class StealthOnPhysicalDamage : OnPhysicalDamage
{
	public override bool OnEnable()
	{
		if (base.Val1Chance)
		{
			ApplyStatus(base.Owner, StatusEnum.Evasion, base.Val2);
			return true;
		}
		return false;
	}
}
