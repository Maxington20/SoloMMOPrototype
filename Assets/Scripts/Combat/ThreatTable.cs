using System.Collections.Generic;
using UnityEngine;

public class ThreatTable : MonoBehaviour
{
    private readonly List<ThreatEntry> entries = new List<ThreatEntry>();

    public void AddThreat(GameObject source, float amount)
    {
        if (source == null || amount <= 0f)
        {
            return;
        }

        ThreatEntry entry = GetEntry(source);

        if (entry == null)
        {
            entry = new ThreatEntry(source, 0f);
            entries.Add(entry);
        }

        entry.Threat += amount;
    }

    public void SetHighestThreat(GameObject source, float bonus)
    {
        if (source == null)
        {
            return;
        }

        float highest = GetHighestThreatValue();

        ThreatEntry entry = GetEntry(source);

        if (entry == null)
        {
            entry = new ThreatEntry(source, 0f);
            entries.Add(entry);
        }

        entry.Threat = highest + bonus;
    }

    public GameObject GetHighestThreatTarget()
    {
        float highest = -1f;
        GameObject target = null;

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            ThreatEntry entry = entries[i];

            if (entry.Source == null)
            {
                entries.RemoveAt(i);
                continue;
            }

            Health health = entry.Source.GetComponent<Health>();
            if (health != null && health.IsDead)
            {
                entries.RemoveAt(i);
                continue;
            }

            if (entry.Threat > highest)
            {
                highest = entry.Threat;
                target = entry.Source;
            }
        }

        return target;
    }

    public void RemoveTarget(GameObject source)
    {
        entries.RemoveAll(e => e.Source == source);
    }

    public void ClearAll()
    {
        entries.Clear();
    }

    private ThreatEntry GetEntry(GameObject source)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Source == source)
            {
                return entries[i];
            }
        }

        return null;
    }

    private float GetHighestThreatValue()
    {
        float highest = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Threat > highest)
            {
                highest = entries[i].Threat;
            }
        }

        return highest;
    }
}