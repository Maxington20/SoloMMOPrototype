using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StatusEffectController playerStatusController;
    [SerializeField] private Transform container;
    [SerializeField] private StatusEffectIconUI iconPrefab;

    private readonly List<StatusEffectIconUI> icons = new List<StatusEffectIconUI>();

    private void OnEnable()
    {
        if (playerStatusController != null)
        {
            playerStatusController.OnStatusEffectsChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (playerStatusController != null)
        {
            playerStatusController.OnStatusEffectsChanged -= Refresh;
        }
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (playerStatusController == null || container == null || iconPrefab == null)
        {
            return;
        }

        var effects = playerStatusController.GetStatusEffectUIInfos();

        while (icons.Count < effects.Count)
        {
            var icon = Instantiate(iconPrefab, container);
            icon.gameObject.SetActive(true);
            icons.Add(icon);
        }

        for (int i = 0; i < icons.Count; i++)
        {
            if (i < effects.Count)
            {
                icons[i].gameObject.SetActive(true);
                icons[i].Refresh(effects[i]);
            }
            else
            {
                icons[i].gameObject.SetActive(false);
            }
        }
    }
}