using UnityEngine;

public class DisplayName : MonoBehaviour
{
    [SerializeField] private string displayName = "Unknown";

    public string Display => displayName;
}