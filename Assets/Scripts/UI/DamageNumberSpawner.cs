using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private FloatingDamageText floatingDamageTextPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, 0f);

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged += SpawnDamageNumber;
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged -= SpawnDamageNumber;
        }
    }

    private void SpawnDamageNumber(int damageAmount, GameObject source)
    {
        if (floatingDamageTextPrefab == null || targetHealth == null)
        {
            return;
        }

        FloatingDamageText damageTextInstance = Instantiate(
            floatingDamageTextPrefab,
            targetHealth.transform.position + spawnOffset,
            Quaternion.identity);

        bool isPlayerTarget = targetHealth.CompareTag("Player") || targetHealth.CompareTag("Adventurer");

        damageTextInstance.Initialize(
            damageAmount,
            targetHealth.transform.position + spawnOffset,
            isPlayerTarget);
    }
}