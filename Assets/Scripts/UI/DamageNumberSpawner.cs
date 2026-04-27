using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private FloatingDamageText floatingTextPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged += OnDamaged;
            targetHealth.OnHealed += OnHealed;
            targetHealth.OnMissed += OnMissed;
            targetHealth.OnDodged += OnDodged;
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged -= OnDamaged;
            targetHealth.OnHealed -= OnHealed;
            targetHealth.OnMissed -= OnMissed;
            targetHealth.OnDodged -= OnDodged;
        }
    }

    private void OnDamaged(int amount, GameObject source)
    {
        var text = Spawn();
        if (text == null) return;

        bool isPlayerTarget = targetHealth.CompareTag("Player");

        text.InitializeDamage(
            amount,
            targetHealth.transform.position + spawnOffset,
            isPlayerTarget);
    }

    private void OnHealed(int amount, GameObject source)
    {
        var text = Spawn();
        if (text == null) return;

        text.InitializeHealing(
            amount,
            targetHealth.transform.position + spawnOffset);
    }

    private void OnMissed(GameObject source)
    {
        var text = Spawn();
        if (text == null) return;

        text.InitializeMiss(targetHealth.transform.position + spawnOffset);
    }

    private void OnDodged(GameObject source)
    {
        var text = Spawn();
        if (text == null) return;

        text.InitializeDodge(targetHealth.transform.position + spawnOffset);
    }

    private FloatingDamageText Spawn()
    {
        if (floatingTextPrefab == null || targetHealth == null)
        {
            return null;
        }

        return Instantiate(
            floatingTextPrefab,
            targetHealth.transform.position + spawnOffset,
            Quaternion.identity);
    }
}