public abstract class OnCritical : AbilityState
{
    protected CreatureState Target;

    public override bool ProcessMessage(GameMessage Message)
    {
        if (Message.Action == GameEvent.CREATURE_ATTACKED && Message.IsCritical && Message.Creature == base.Owner)
        {
            Target = Message.SecondCreature;
            // Ensure critical hit is processed and communicated
            HandleCriticalHit(Message);
            return OnEnable();
        }
        return false;
    }

    private void HandleCriticalHit(GameMessage Message)
    {
        // Logic to handle critical hit, e.g., updating UI, notifying server
        // Ensure synchronization with server
        ReportCriticalHit(Message);
    }

    private void ReportCriticalHit(GameMessage Message)
    {
        // Send critical hit status to server or other clients
        // Example: Server.SendCriticalHitStatus(Message);
    }
}
