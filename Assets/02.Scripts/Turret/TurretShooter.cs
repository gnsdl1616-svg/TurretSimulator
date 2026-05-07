using UnityEngine;

// projectile 발사 Data 포함, 실제 발사까지 담당
public class TurretShooter : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private TurretAimingController aimingController;
    [SerializeField] private Transform muzzlePointTransform;
    [SerializeField] private ProjectilePool projectilePool;

    [Header("Fire Stat Status")]
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;

    private float lastFireTime = -999f;

    private void Awake()
    {
        if (aimingController == null)
        {
            aimingController = GetComponent<TurretAimingController>();
        }

        if (muzzlePointTransform == null && aimingController != null)
        {
            muzzlePointTransform = aimingController.MuzzlePointTransform;
        }
    }

    public void TickShoot()
    {
        if (!CanShoot())
        {
            return;
        }

        FireProjectile();
    }

    private bool CanShoot()
    {
        if (aimingController == null) return false;
        if (!aimingController.HasTarget) return false;
        if (!aimingController.IsLockOnTarget()) return false;
        if (muzzlePointTransform == null) return false;
        if (projectilePool == null) return false;
        if (Time.time < lastFireTime + fireInterval) return false;

        return true;
    }

    private void FireProjectile()
    {
        lastFireTime = Time.time;

        Projectile projectile = projectilePool.GetProjectile(
            muzzlePointTransform.position,
            muzzlePointTransform.rotation,
            projectileSpeed,
            projectileLifeTime
        );

        if (projectile == null)
        {
            Debug.LogWarning("사용 가능한 Projectile이 없습니다.");
        }
    }
}
