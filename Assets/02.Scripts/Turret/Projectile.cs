using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileLifeTime;

    private ProjectilePool ownerPool;
    private float lifeTimer;
    private bool isInitialized;

    public void Init(ProjectilePool pool, float speed, float lifeTime)
    {
        ownerPool = pool;
        projectileSpeed = speed;
        projectileLifeTime = lifeTime;

        lifeTimer = 0f;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        transform.position += transform.forward * projectileSpeed * Time.deltaTime;

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= projectileLifeTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeHit();
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (!isInitialized)
        {
            return;
        }

        isInitialized = false;

        if (ownerPool != null)
        {
            ownerPool.ReturnProjectile(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
