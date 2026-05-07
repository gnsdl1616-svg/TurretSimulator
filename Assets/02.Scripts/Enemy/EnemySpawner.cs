using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private Transform turretTargetTransform;
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Orbit")]
    [SerializeField] private bool rotateAroundTurret = true;
    [SerializeField] private float orbitSpeed = 30f;

    private float lastSpawnTime = -999f;

    private void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    private void Update()
    {
        TickOrbit();
        TickSpawn();
    }

    private void TickOrbit()
    {
        if (!rotateAroundTurret)
        {
            return;
        }

        if (turretTargetTransform == null)
        {
            return;
        }

        transform.RotateAround(
            turretTargetTransform.position,
            Vector3.up,
            orbitSpeed * Time.deltaTime
        );
    }

    private void TickSpawn()
    {
        if (enemyPool == null)
        {
            return;
        }

        if (turretTargetTransform == null)
        {
            return;
        }

        if (Time.time < lastSpawnTime + spawnInterval)
        {
            return;
        }

        lastSpawnTime = Time.time;

        enemyPool.GetEnemy(
            spawnPoint.position,
            spawnPoint.rotation,
            turretTargetTransform
        );
    }
}
