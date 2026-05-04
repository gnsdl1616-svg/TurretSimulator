using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectillifeTime;

    public void Init(float speed, float lifeTime)
    {
        projectileSpeed = speed;
        projectillifeTime = lifeTime;

        Destroy(gameObject, projectillifeTime);
    }

    private void Update()
    {
        this.transform.position += transform.forward * projectileSpeed * Time.deltaTime;
    }
}
