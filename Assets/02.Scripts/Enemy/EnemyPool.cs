using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [Header("Pool Ref")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Transform poolParent;

    [Header("Pool Setting")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool canExpand = true;

    private readonly Queue<Enemy> enemyQueue = new Queue<Enemy>();

    private void Awake()
    {
        if (poolParent == null)
        {
            poolParent = transform;
        }

        CreateInitialPool();
    }

    private void CreateInitialPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            Enemy enemy = CreateNewEnemy();
            ReturnEnemy(enemy);
        }
    }

    private Enemy CreateNewEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, poolParent);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    public Enemy GetEnemy(Vector3 position, Quaternion rotation, Transform moveTarget)
    {
        Enemy enemy = null;

        if (enemyQueue.Count > 0)
        {
            enemy = enemyQueue.Dequeue();
        }
        else if (canExpand)
        {
            enemy = CreateNewEnemy();
        }

        if (enemy == null)
        {
            return null;
        }

        enemy.transform.SetPositionAndRotation(position, rotation);
        enemy.gameObject.SetActive(true);
        enemy.Init(this, moveTarget);

        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolParent);
        enemyQueue.Enqueue(enemy);
    }
}
