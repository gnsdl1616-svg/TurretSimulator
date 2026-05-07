using UnityEngine;

namespace ProtoType
{
    public class TurretSimulatorController : MonoBehaviour
    {
        [Header("orbitPivot (궤도 중심)")]
        [SerializeField] Transform TargetRailPivot;
        [SerializeField] private Transform turretHeadBodyTransform;
        [SerializeField] private Transform pitchPivotTransform;

        [Header("MuzzlePoint")]
        [SerializeField] private Transform muzzlePointTransform;

        [Header("조준 허용 각도")]
        [SerializeField] private float fireAngleThreshold = 5.0f;

        [Header("Bodies (회전 할 오브젝트들)")]
        [SerializeField] private Transform targetDroneBodyTransform;

        [Header("드론 이동 제어")]
        [SerializeField] private bool isDronMove;

        public Vector3 droneEulerDirection = new Vector3(1f, 2f, 3f);

        [Header("DroneSpeed")]
        public float targetRailSpeed = 30.0f;
        public float droneSpinSpeed = 90.0f;

        [Header("TurretSpinSpeed")]
        public float turretHeadSpinSpeed = 40.0f;
        public float turretBarrelAngleSpeed = 20.0f;

        private Vector3 droneCurrentEulerAngles;

        [Header("BarrelAngleLimit")]
        [SerializeField] private float minPitch = -45.0f;
        [SerializeField] private float maxPitch = 20.0f;

        [Header("Fire")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireInterval = 0.5f;
        [SerializeField] private float projectileSpeed = 12.0f;
        [SerializeField] private float projectilelifeTime = 3.0f;

        private float lastFireTime = -999.0f;

        private void Start()
        {
            isDronMove = true;
        }

        private void Update()
        {
            DroneRotate();
            RotateYawToTarget();
            RotatePitchToTarget();
            FireProjectile();
        }

        private void DroneRotate()
        {
            if (isDronMove)
            {
                TargetRailPivot.Rotate(Vector3.up, targetRailSpeed * Time.deltaTime, Space.Self);

                droneCurrentEulerAngles += droneEulerDirection * droneSpinSpeed * Time.deltaTime;

                droneCurrentEulerAngles.x = Mathf.Repeat(droneCurrentEulerAngles.x, 360f);
                droneCurrentEulerAngles.y = Mathf.Repeat(droneCurrentEulerAngles.y, 360f);
                droneCurrentEulerAngles.z = Mathf.Repeat(droneCurrentEulerAngles.z, 360f);

                targetDroneBodyTransform.localEulerAngles = droneCurrentEulerAngles;
            }
        }

        private void RotateYawToTarget()
        {
            Vector3 targetDirection = targetDroneBodyTransform.position - turretHeadBodyTransform.position;
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
            Vector3 targetDirection = targetDroneBodyTransform.position - pitchPivotTransform.position;

            if (targetDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 localDirection = turretHeadBodyTransform.InverseTransformDirection(targetDirection);

            float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude;
            float targetPitch = Mathf.Atan2(localDirection.y, horizontalDistance) * Mathf.Rad2Deg;

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

        private bool IsLockOnTarget()
        {
            Vector3 targetDirection = targetDroneBodyTransform.position - muzzlePointTransform.position;
            float angle = Vector3.Angle(muzzlePointTransform.forward, targetDirection);

            Debug.DrawRay(muzzlePointTransform.position, muzzlePointTransform.forward * 5f, Color.red);
            Debug.DrawLine(muzzlePointTransform.position, targetDroneBodyTransform.position, Color.green);

            return angle <= fireAngleThreshold;
        }

        private void FireProjectile()
        {
            if (!IsLockOnTarget())
            {
                return;
            }

            if (Time.time < lastFireTime + fireInterval)
            {
                return;
            }

            lastFireTime = Time.time;

            GameObject newProjectile = Instantiate(
                projectilePrefab,
                muzzlePointTransform.position,
                muzzlePointTransform.rotation
            );

            Projectile projectile = newProjectile.GetComponent<Projectile>();

            if (projectile != null)
            {
                // 현재 프로젝트에서 Projectile.cs가 Pooling 버전이면 이 Init 시그니처와 다릅니다.
                // 이 원본 백업 스크립트는 과제 초기 통합 구조 보관용입니다.
            }
        }
    }
}
