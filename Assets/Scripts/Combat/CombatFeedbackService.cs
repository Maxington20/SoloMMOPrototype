public static class CombatFeedbackService
{
    public static void QueueImpactFeedback(Health targetHealth, AbilityData ability)
    {
        if (targetHealth == null)
        {
            return;
        }

        CombatFeedbackReceiver feedbackReceiver = targetHealth.GetComponent<CombatFeedbackReceiver>();

        if (feedbackReceiver == null)
        {
            return;
        }

        feedbackReceiver.QueueAbilityImpactFeedback(ability);
    }
}