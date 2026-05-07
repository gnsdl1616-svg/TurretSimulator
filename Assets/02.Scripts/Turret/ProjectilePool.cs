using System.Collections.Generic;
using UnityEngine;

// 탄환을 미리 생성해두고 재사용하는 Pool
public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Ref")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform poolParent;

    [Header("Pool Setting")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private bool canExpand = true;

    private readonly Queue<Projectile> projectileQueue = new Queue<Projectile>();

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
            Projectile projectile = CreateNewProjectile();
            ReturnProjectile(projectile);
        }
    }

    private Projectile CreateNewProjectile()
    {
        Projectile projectile = Instantiate(projectilePrefab, poolParent);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    public Projectile GetProjectile(Vector3 position, Quaternion rotation, float speed, float lifeTime)
    {
        Projectile projectile = null;

        if (projectileQueue.Count > 0)
        {
            projectile = projectileQueue.Dequeue();
        }
        else if (canExpand)
        {
            projectile = CreateNewProjectile();
        }

        if (projectile == null)
        {
            return null;
        }

        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.gameObject.SetActive(true);
        projectile.Init(this, speed, lifeTime);

        return projectile;
    }

    public void ReturnProjectile(Projectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(poolParent);
        projectileQueue.Enqueue(projectile);
    }
}
