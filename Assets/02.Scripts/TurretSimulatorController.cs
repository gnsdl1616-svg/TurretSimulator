using UnityEngine;

public class TurretSimulatorController : MonoBehaviour
{
    [Header("orbitPivot (궤도 중심)")]
    [SerializeField] Transform TargetRailPivot; // Plane or TargetRailPivot 생성
    [SerializeField] private Transform turretHeadBodyTransform; // 
    [SerializeField] private Transform pitchPivotTransform; // 

    [Header("MuzzlePoint")]
    [SerializeField] private Transform muzzlePointTransform;
    [Header("조준 허용 각도")]
    [SerializeField] private float fireAngleThreshold = 5.0f;

    [Header("Bodies (회전 할 오브젝트들)")]
    [SerializeField] private Transform targetDroneBodyTransform; // TargetRailPivot 기준 공전

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

    void Start()
    {
        isDronMove = true;
    }

    void Update()
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
        // 포탑머리에서 타겟까지의 방향 구하기
        Vector3 targetDirection = targetDroneBodyTransform.position - turretHeadBodyTransform.position;

        // 높이값 제거
        targetDirection.y = 0f;

        // sqrMagnitude 제곱값, magnitude는 루트를 사용하기때문에 비용이 크다.
        // 단순 제곱 계산으로 거리 계산, 비교값도 제곱으로 해야하기때문에 0.1f이나 0.01f보다 0.0001f로 해야 실제거리 근소값으로 볼수 있다.
        // 근소값이 되면 조준 완료 판정
        if (targetDirection.sqrMagnitude < 0.0001f) 
        {
            Debug.Log("Yaw 조준 완료");
            return;
        }
        
        // Atan2 는 방향 벡터를 각도로 바꿔주는 함수 반환값은 Radian
        // Yaw 의 각도를 구할때 Vector3.forward(0,0,1), forward 가 보통 z축이기 때문에 Atan2(x, z)로 계산
        // x값이 좌우 z값이 앞뒤 방향
        // Rad2Deg -> Radian 을 도(Degree)로 변환
        // Unity 인스펙터의 Rotation 이나 Quaternion.Euler()는 보통 도 단위를 사용
        float targetYaw = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

        // 현재 포탑 머리의 y축 회전 값
        float currentYaw = turretHeadBodyTransform.eulerAngles.y;

        // Mathf.MoveTowardsAngle(float current, float target, float maxDelta)
        // current(현재위치) 에서 target(목표) 까지 이동하는데 걸리는 maxDelta(시간[스피드 * 시간]) -> DeltaTime마다 이동할 각도 계산
        float newYaw = Mathf.MoveTowardsAngle(
            currentYaw,
            targetYaw,
            turretHeadSpinSpeed * Time.deltaTime
            );

        // y축 회전만 적용
        turretHeadBodyTransform.eulerAngles = new Vector3 (0f, newYaw, 0f);
    }

    private void RotatePitchToTarget()
    {
        // PitchPivot(BarrelBody) 기준에서 타겟 방향을 구하기. -> world 기준 방향으로 구해짐
        Vector3 targetDirection = targetDroneBodyTransform.position - pitchPivotTransform.position;

        if (targetDirection.sqrMagnitude < 0.0001f)
        {            
            return;
        }

        // world 기준 방향을 pitchPivot의 로컬방향으로 변환하여 YawPivot에서 회전해서 좌표계 기준이 섞이는 것을 방지
        Vector3 localDirection = turretHeadBodyTransform.InverseTransformDirection(targetDirection);

        // 수평 전방 거리와 높이 차이를 이용한 Pitch 각도 계산
        float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude;
        float targetPitch = Mathf.Atan2(localDirection.y, horizontalDistance) * Mathf.Rad2Deg;

        // 위로 회전은 x축 기준으로 회전
        // 위로 드는 방향이 -값
        targetPitch = -targetPitch;

        // Pitch 각도 제한
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // 현재 local x 회전값을 -180~180 범위로 변환
        float currentPitch = pitchPivotTransform.localEulerAngles.x;
        // Mathf.DeltaAngle(0f, currentPitch)
        // 0도 기준에서 currentPitch가 얼마나 차이 나는지 “가장 짧은 각도 차이”로 계산하는 것.
        // -180 ~ 180도 안에서 계산
        currentPitch = Mathf.DeltaAngle(0f, currentPitch);

        float newPitch = Mathf.MoveTowardsAngle(
            currentPitch,
            targetPitch,
            turretBarrelAngleSpeed * Time.deltaTime
            );

        // x축 회전만 적용
        pitchPivotTransform.localEulerAngles = new Vector3(newPitch, 0f, 0f);

        /* 실패 흔적코드
         * Vector3 pitchDirection = new Vector3(0, targetDirection.y, 0);

        float horizontalDistance = targetDroneBodyTransform.position.y - turretHeadBodyTransform.position.y;

        float targetPitch = Mathf.Atan2(targetDirection.y, horizontalDistance) * Mathf.Rad2Deg;

        float currentPitch = BarrelBodyTransform.transform.eulerAngles.y;

        turretBarrelEulerAngles = BarrelBodyTransform.transform.eulerAngles;

        if (currentPitch != targetPitch)
        {
            turretHeadBodyTransform.Rotate(pitchDirection, turretBarrelAngleSpeed * Time.deltaTime, Space.Self);

            turretBarrelEulerAngles += pitchDirection * targetPitch * Time.deltaTime;

            turretBarrelEulerAngles.x = Mathf.Clamp(turretBarrelEulerAngles.y, minPitch, maxPitch);

            BarrelBodyTransform.transform.eulerAngles = turretBarrelEulerAngles;
        }*/
    }

    private bool IsLockOnTarget()
    {
        Vector3 targetDirection = targetDroneBodyTransform.position - muzzlePointTransform.position;
        float angle = Vector3.Angle(muzzlePointTransform.forward, targetDirection);

        // AI 검증 코드
        Debug.DrawRay(muzzlePointTransform.position, muzzlePointTransform.forward * 5f, Color.red);
        Debug.DrawLine(muzzlePointTransform.position, targetDroneBodyTransform.position, Color.green);

        return angle <= fireAngleThreshold;

        /*if (angle <= fireAngleThreshold)
        {
            return true;
        }
        return false;*/
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

        GameObject newProjectile = Instantiate(projectilePrefab, muzzlePointTransform.position, muzzlePointTransform.rotation);

        /* AI 검증 코드
         * Debug.Log("MuzzlePoint Position: " + muzzlePointTransform.position);
        Debug.Log("Projectile Spawn Position: " + newProjectile.transform.position);

        Debug.Log("발사체 생성됨: " + newProjectile.name);*/

        /* AI 검증 코드
         * GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = muzzlePointTransform.position;
        marker.transform.localScale = Vector3.one * 0.15f;
        Destroy(marker, 1f);*/

        Projectile projectile = newProjectile.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(projectileSpeed, projectilelifeTime);
        }
        else
        {
            Debug.LogWarning("Projectile 컴포넌트가 프리팹에 없습니다.");
        }
    }
}
