using System;
using UnityEngine;

[Serializable]
public class QuestWorldInteractObjective
{
    [SerializeField] private WorldInteractableData interactable;

    public WorldInteractableData Interactable => interactable;

    public string DisplayName
    {
        get
        {
            if (interactable != null && !string.IsNullOrWhiteSpace(interactable.DisplayName))
            {
                return interactable.DisplayName;
            }

            return "Object";
        }
    }
}