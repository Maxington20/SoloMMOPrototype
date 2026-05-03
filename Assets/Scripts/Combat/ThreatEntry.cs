using UnityEngine;

public class ThreatEntry
{
    public GameObject Source;
    public float Threat;

    public ThreatEntry(GameObject source, float threat)
    {
        Source = source;
        Threat = threat;
    }
}