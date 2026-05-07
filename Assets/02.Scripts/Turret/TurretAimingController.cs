using UnityEngine;

// 조준, 각도 정보 제공
public class TurretAimingController : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("좌우 회전 대상. TurretHead or YawPivot")]
    [SerializeField] private Transform turretHeadBodyTransform;

    [Tooltip("상하 회전 대상. PitchPivot")]
    [SerializeField] private Transform pitchPivotTransform;

    [Tooltip("MuzzlePoint")]
    [SerializeField] private Transform muzzlePointTransform;

    [Tooltip("조준 대상. Target")]
    [SerializeField] private Transform targetTransform;

    [Header("Aiming Status")]
    [Tooltip("조준 완료 허용 각도차")]
    [SerializeField] private float fireAngleThreshold = 5.0f;

    [Tooltip("위로 올릴 수 있는 허용 각도")]
    [SerializeField] private float minPitch = -45f;

    [Tooltip("아래로 내릴 수 있는 허용 각도")]
    [SerializeField] private float maxPitch = 20f;

    [Header("터렛 회전 속도")]
    [SerializeField] private float turretHeadSpinSpeed = 100f;
    [SerializeField] private float turretBarrelAngleSpeed = 40f;

    public bool HasTarget => targetTransform != null && targetTransform.gameObject.activeInHierarchy;
    public Transform TargetTransform => targetTransform;
    public Transform MuzzlePointTransform => muzzlePointTransform;

    private void Awake()
    {
        if (turretHeadBodyTransform == null)
        {
            turretHeadBodyTransform = transform;
        }

        if (pitchPivotTransform == null)
        {
            pitchPivotTransform = transform;
        }

        if (muzzlePointTransform == null)
        {
            muzzlePointTransform = transform;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
    }

    public void TickAim()
    {
        if (!HasTarget)
        {
            return;
        }

        if (!HasRequiredReferences())
        {
            return;
        }

        RotateYawToTarget();
        RotatePitchToTarget();
    }

    private bool HasRequiredReferences()
    {
        return turretHeadBodyTransform != null
            && pitchPivotTransform != null
            && muzzlePointTransform != null
            && targetTransform != null;
    }

    private void RotateYawToTarget()
    {
        Vector3 targetDirection = targetTransform.position - turretHeadBodyTransform.position;

        // Yaw는 좌우 회전만 해야 하므로 높이값 제거
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float targetYaw = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
        float currentYaw = turretHeadBodyTransform.eulerAngles.y;

        float newYaw = Mathf.MoveTowardsAngle(
            currentYaw,
            targetYaw,
            turretHeadSpinSpeed * Time.deltaTime
        );

        turretHeadBodyTransform.eulerAngles = new Vector3(0f, newYaw, 0f);
    }

    private void RotatePitchToTarget()
    {
        Vector3 targetDirection = targetTransform.position - pitchPivotTransform.position;

        if (targetDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // YawPivot 기준 로컬 방향으로 변환
        Vector3 localDirection = turretHeadBodyTransform.InverseTransformDirection(targetDirection);

        float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude;
        float targetPitch = Mathf.Atan2(localDirection.y, horizontalDistance) * Mathf.Rad2Deg;

        // 현재 구조에서는 위로 드는 방향이 X축 음수
        targetPitch = -targetPitch;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        float currentPitch = pitchPivotTransform.localEulerAngles.x;
        currentPitch = Mathf.DeltaAngle(0f, currentPitch);

        float newPitch = Mathf.MoveTowardsAngle(
            currentPitch,
            targetPitch,
            turretBarrelAngleSpeed * Time.deltaTime
        );

        pitchPivotTransform.localEulerAngles = new Vector3(newPitch, 0f, 0f);
    }

    public bool IsLockOnTarget()
    {
        if (!HasTarget)
        {
            return false;
        }

        if (muzzlePointTransform == null)
        {
            return false;
        }

        Vector3 targetDirection = targetTransform.position - muzzlePointTransform.position;

        if (targetDirection.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        float angle = Vector3.Angle(muzzlePointTransform.forward, targetDirection);

        Debug.DrawRay(muzzlePointTransform.position, muzzlePointTransform.forward * 5f, Color.red);
        Debug.DrawLine(muzzlePointTransform.position, targetTransform.position, Color.green);

        return angle <= fireAngleThreshold;
    }
}
