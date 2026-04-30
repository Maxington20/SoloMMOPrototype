using System;
using UnityEngine;

[Serializable]
public class ClassAbilityUnlock
{
    [SerializeField] private int unlockLevel = 1;
    [SerializeField] private AbilityData ability;
    [SerializeField] private bool autoAssignToHotbar = true;

    public int UnlockLevel => Mathf.Max(1, unlockLevel);
    public AbilityData Ability => ability;
    public bool AutoAssignToHotbar => autoAssignToHotbar;
}