using UnityEngine;

// 확장성 고려
// 각 컴포넌트 실행 순서를 명확히 관리하는 스크립트(총괄)
[RequireComponent(typeof(TurretAimingController))]
[RequireComponent(typeof(TurretTargetScanner))]
[RequireComponent(typeof(TurretShooter))]
public class TurretController : MonoBehaviour
{
    // 각각 컴포넌트가 Update에서 각자 실행하면 순서가 꼬일 수 있다.
    // 총괄 스크립트 하나에 순차적으로 실행하면 순서 정리가 깔끔해진다.
    [Header("Ref")]
    [SerializeField] private TurretTargetScanner targetScanner;
    [SerializeField] private TurretAimingController aimingController;
    [SerializeField] private TurretShooter shooter;

    private void Awake()
    {
        if (targetScanner == null)
        {
            targetScanner = GetComponent<TurretTargetScanner>();
        }

        if (aimingController == null)
        {
            aimingController = GetComponent<TurretAimingController>();
        }

        if (shooter == null)
        {
            shooter = GetComponent<TurretShooter>();
        }
    }

    private void Update()
    {
        if (targetScanner == null || aimingController == null || shooter == null)
        {
            return;
        }

        targetScanner.TickScan();

        aimingController.SetTarget(targetScanner.CurrentTarget);
        aimingController.TickAim();

        shooter.TickShoot();
    }
}
