using System.Collections;
using UnityEngine;

// projectile 발사 Data 포함, 실제 발사까지 담당
public class TurretShooter : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private TurretAimingController aimingController;
    [SerializeField] private Transform muzzlePointTransform;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private TurretFireEffect fireEffect;
    [SerializeField] private TurretBarrelCoolingEffect barrelCoolingEffect;

    [Header("Fire Stat Status")]
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;
    [SerializeField] private int overHeatingFireCount = 5;
    
    private int currentFireCount = 0;
    private float lastFireTime = -999f;
    private bool isFiring = false;
    private bool isCooling = false;

    public Transform MuzzlePointTransform => muzzlePointTransform;

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

        if (fireEffect == null)
        {
            fireEffect = GetComponent<TurretFireEffect>();
        }

        if (barrelCoolingEffect == null)
        {
            barrelCoolingEffect = GetComponent <TurretBarrelCoolingEffect>();
        }
    }

    public void TickShoot()
    {
        if (!CanShoot())
        {
            return;
        }

        // FireProjectile();
        StartCoroutine(FireRoutine());
    }

    private bool CanShoot()
    {
        if (isFiring) return false;
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
        Projectile projectile = projectilePool.GetProjectile(
            muzzlePointTransform.position,
            muzzlePointTransform.rotation,
            projectileSpeed,
            projectileLifeTime
        );

        currentFireCount++;

        if (projectile == null)
        {
            Debug.LogWarning("사용 가능한 Projectile이 없습니다.");
        }
    }

    private IEnumerator FireRoutine()
    {
        isFiring = true;

        // 코루틴 중복 실행을 막기 위해 발사 시점을 먼저 기록
        lastFireTime = Time.time;

        FireProjectile();

        if (barrelCoolingEffect != null)
        {
            float heatRatio = currentFireCount / (float)overHeatingFireCount;
            barrelCoolingEffect.SetHeatRatio(heatRatio);
        }

        if (fireEffect != null)
        {
            yield return fireEffect.FireEffectRoutine();
        }

        if (currentFireCount >= overHeatingFireCount)
        {
            yield return CoolingSystemRoutine();
        }

        isFiring = false;
    }

    private IEnumerator CoolingSystemRoutine()
    {
        isCooling = true;

        if (barrelCoolingEffect != null)
        {
            yield return barrelCoolingEffect.CoolingEffectRoutine();
        }

        currentFireCount = 0;
        isCooling = false;
    }
}
