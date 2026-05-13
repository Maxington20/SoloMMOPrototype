using System.Collections.Generic;
using UnityEngine;

public class TrainerNPC : MonoBehaviour
{
    [Header("Fallback Trainer")]
    [SerializeField] private string trainerName = "Trainer";
    [SerializeField] private CharacterClassData fallbackClassRestriction;
    [SerializeField] private List<ClassAbilityUnlock> fallbackTrainableAbilities = new List<ClassAbilityUnlock>();

    private readonly List<ClassAbilityUnlock> activeTrainableAbilities = new List<ClassAbilityUnlock>();

    private CharacterClassData activeClassRestriction;

    public string TrainerName => trainerName;
    public CharacterClassData ClassRestriction => activeClassRestriction;
    public IReadOnlyList<ClassAbilityUnlock> TrainableAbilities => activeTrainableAbilities;

    private void Awake()
    {
        RebuildFromFallback();
    }

    public void ApplyNpcData(NpcData npcData)
    {
        activeTrainableAbilities.Clear();

        if (npcData == null)
        {
            RebuildFromFallback();
            return;
        }

        if (!string.IsNullOrWhiteSpace(npcData.DisplayName))
        {
            trainerName = npcData.DisplayName;
        }

        activeClassRestriction = npcData.TrainerClassRestriction;

        if (npcData.TrainableAbilities != null)
        {
            for (int i = 0; i < npcData.TrainableAbilities.Count; i++)
            {
                ClassAbilityUnlock unlock = npcData.TrainableAbilities[i];

                if (unlock != null && unlock.Ability != null)
                {
                    activeTrainableAbilities.Add(unlock);
                }
            }
        }
    }

    public void OpenTrainer()
    {
        if (TrainerWindowUI.Instance == null)
        {
            PostSystem("Trainer UI is missing.");
            return;
        }

        TrainerWindowUI.Instance.Open(this);
    }

    public bool PlayerMeetsClassRequirement(PlayerClassController playerClassController)
    {
        if (activeClassRestriction == null)
        {
            return true;
        }

        if (playerClassController == null)
        {
            return false;
        }

        return playerClassController.SelectedClass == activeClassRestriction;
    }

    private void RebuildFromFallback()
    {
        activeTrainableAbilities.Clear();
        activeClassRestriction = fallbackClassRestriction;

        if (fallbackTrainableAbilities == null)
        {
            return;
        }

        for (int i = 0; i < fallbackTrainableAbilities.Count; i++)
        {
            ClassAbilityUnlock unlock = fallbackTrainableAbilities[i];

            if (unlock != null && unlock.Ability != null)
            {
                activeTrainableAbilities.Add(unlock);
            }
        }
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}