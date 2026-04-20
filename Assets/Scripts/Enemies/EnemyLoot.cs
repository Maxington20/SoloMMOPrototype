using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private LootDrop lootPrefab;

    public void DropLoot()
    {
        if (enemyData == null || lootPrefab == null)
        {
            return;
        }

        int gold = Random.Range(enemyData.MinGold, enemyData.MaxGold + 1);

        if (gold <= 0)
        {
            return;
        }

        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            0.2f,
            Random.Range(-0.5f, 0.5f)
        );

        LootDrop drop = Instantiate(lootPrefab, spawnPos, Quaternion.identity);
        drop.Initialize(gold);
    }
}