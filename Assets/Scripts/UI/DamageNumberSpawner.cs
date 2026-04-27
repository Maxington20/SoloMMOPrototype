using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private FloatingDamageText floatingDamageTextPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged += SpawnDamageNumber;
            targetHealth.OnHealed += SpawnHealingNumber;
            targetHealth.OnMissed += SpawnMissText;
            targetHealth.OnDodged += SpawnDodgeText;
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged -= SpawnDamageNumber;
            targetHealth.OnHealed -= SpawnHealingNumber;
            targetHealth.OnMissed -= SpawnMissText;
            targetHealth.OnDodged -= SpawnDodgeText;
        }
    }

    private void SpawnDamageNumber(int damageAmount, GameObject source)
    {
        FloatingDamageText textInstance = SpawnTextInstance();

        if (textInstance == null)
        {
            return;
        }

        bool isPlayerTarget = targetHealth.CompareTag("Player") || targetHealth.CompareTag("Adventurer");

        textInstance.InitializeDamage(
            damageAmount,
            targetHealth.transform.position + spawnOffset,
            isPlayerTarget);
    }

    private void SpawnHealingNumber(int healingAmount, GameObject source)
    {
        FloatingDamageText textInstance = SpawnTextInstance();

        if (textInstance == null)
        {
            return;
        }

        textInstance.InitializeHealing(
            healingAmount,
            targetHealth.transform.position + spawnOffset);
    }

    private void SpawnMissText(GameObject source)
    {
        FloatingDamageText textInstance = SpawnTextInstance();

        if (textInstance == null)
        {
            return;
        }

        textInstance.InitializeMiss(targetHealth.transform.position + spawnOffset);
    }

    private void SpawnDodgeText(GameObject source)
    {
        FloatingDamageText textInstance = SpawnTextInstance();

        if (textInstance == null)
        {
            return;
        }

        textInstance.InitializeDodge(targetHealth.transform.position + spawnOffset);
    }

    private FloatingDamageText SpawnTextInstance()
    {
        if (floatingDamageTextPrefab == null || targetHealth == null)
        {
            return null;
        }

        return Instantiate(
            floatingDamageTextPrefab,
            targetHealth.transform.position + spawnOffset,
            Quaternion.identity);
    }
}