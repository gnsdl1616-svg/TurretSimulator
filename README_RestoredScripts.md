# Turret Simulator - Restored Scripts

실수로 삭제된 작업물을 복구하기 위한 스크립트 묶음입니다.

## 포함 파일

### 터렛 컴포넌트 분리 구조

- `TurretController.cs`
- `TurretTargetScanner.cs`
- `TurretAimingController.cs`
- `TurretShooter.cs`

### Pooling 구조

- `ProjectilePool.cs`
- `Projectile.cs`
- `EnemyPool.cs`
- `Enemy.cs`
- `EnemySpawner.cs`

### 원본 통합 스크립트 백업

- `ProtoType_TurretSimulatorController.cs`

## Unity 배치 요약

터렛 오브젝트에는 다음 컴포넌트를 같이 붙입니다.

```text
TurretController
TurretTargetScanner
TurretAimingController
TurretShooter
```

Projectile Pool 오브젝트에는 다음 컴포넌트를 붙입니다.

```text
ProjectilePool
```

Enemy Pool 오브젝트에는 다음 컴포넌트를 붙입니다.

```text
EnemyPool
```

EnemySpawner 오브젝트에는 다음 컴포넌트를 붙입니다.

```text
EnemySpawner
```

## 필수 설정

### Enemy

- Collider 필요
- Layer를 Enemy로 지정
- `TurretTargetScanner`의 Target Layer Mask에 Enemy 체크

### Projectile

- Collider 필요
- Collider의 Is Trigger 체크 권장
- Rigidbody 추가 권장
- Rigidbody는 Use Gravity 해제, Is Kinematic 체크 권장

## 주의

`ProtoType_TurretSimulatorController.cs`는 원본 통합 구조 백업용입니다.  
현재 Pooling 구조에서는 `TurretController`, `TurretTargetScanner`, `TurretAimingController`, `TurretShooter`를 사용하는 쪽이 우선입니다.
