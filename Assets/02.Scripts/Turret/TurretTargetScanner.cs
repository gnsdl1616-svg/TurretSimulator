using UnityEngine;

// 다가오는 Target 탐색 후 AimingController에게 정보 전달
public class TurretTargetScanner : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("탐지 중심 위치")]
    [SerializeField] private Transform turretTransform;

    [Header("Scanner Stat Status")]
    [Tooltip("타겟 탐지 범위")]
    [SerializeField] private float scanRadius = 10f;

    [Tooltip("타겟 탐지 주기")]
    [SerializeField] private float scanInterval = 0.1f;

    [Header("TargetLayer")]
    [Tooltip("탐지할 target Layer를 지정")]
    [SerializeField] private LayerMask targetLayerMask;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    // LayerMask가 1차 탐지, Collider로 실제 감지
    private readonly Collider[] hitBuffer = new Collider[32];

    private Transform currentTarget;
    private float lastScanTime = -999f;

    public Transform CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null;

    private void Awake()
    {
        if (turretTransform == null)
        {
            turretTransform = transform;
        }
    }

    // TurretController에서 호출할 탐색 함수
    public void TickScan()
    {
        if (Time.time < lastScanTime + scanInterval)
        {
            return;
        }

        lastScanTime = Time.time;
        ScanClosestTarget();
    }

    // 필요할 때 강제로 즉시 탐색하고 싶을 때 사용
    [ContextMenu("ForceScan")]
    public void ForceScan()
    {
        Debug.Log("스캔 실행");
        ScanClosestTarget();
    }

    // Closest = 가장 가까운
    private void ScanClosestTarget()
    {
        if (turretTransform == null)
        {
            currentTarget = null;
            return;
        }

        // Physics.OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask)
        // position에서 radius 범위 안에 targetLayer로 지정된 Collider를 가진 오브젝트 수를 int로 반환
        int hitCount = Physics.OverlapSphereNonAlloc(
            turretTransform.position,
            scanRadius,
            hitBuffer,
            targetLayerMask
        );

        Transform closestTarget = null;
        float closestSqrDistance = scanRadius * scanRadius;

        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider = hitBuffer[i];

            if (targetCollider == null)
            {
                continue;
            }

            Transform target = targetCollider.transform;

            Vector3 direction = target.position - turretTransform.position;
            float sqrDistance = direction.sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestTarget = target;
            }
        }

        currentTarget = closestTarget;
    }

    // 디버깅용 기즈모
    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
        {
            return;
        }

        Transform center = turretTransform != null ? turretTransform : transform;

        Gizmos.DrawWireSphere(center.position, scanRadius);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center.position, currentTarget.position);
        }
    }
}
