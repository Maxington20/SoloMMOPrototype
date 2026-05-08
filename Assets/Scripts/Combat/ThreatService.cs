using UnityEngine;

public static class ThreatService
{
    public static void ApplyThreat(GameObject sourceObject, GameObject targetObject, AbilityData ability, int damageAmount)
    {
        if (sourceObject == null || targetObject == null)
        {
            return;
        }

        ThreatTable threatTable = targetObject.GetComponent<ThreatTable>();

        if (threatTable == null)
        {
            return;
        }

        float threat = damageAmount;

        if (ability != null)
        {
            threat *= ability.ThreatMultiplier;
            threat += ability.BonusThreat;

            if (ability.IsTaunt)
            {
                threatTable.SetHighestThreat(sourceObject, 50f);
                return;
            }
        }

        threatTable.AddThreat(sourceObject, threat);
    }
}