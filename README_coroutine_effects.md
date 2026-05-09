# Coroutine Effects Update

Unity 자동 포탑 시뮬레이터 프로젝트에서 코루틴을 활용해 추가한 연출 작업을 정리한 README입니다.

이번 업데이트의 핵심은 **시간 흐름이 필요한 연출을 `IEnumerator` 코루틴으로 분리**한 것입니다.

---

## 1. 작업 개요

기존 프로젝트는 다음 기능까지 구현되어 있었습니다.

- 터렛 컴포넌트 분리
- 타겟 탐색
- Yaw / Pitch 조준
- Projectile 발사
- Projectile Pooling
- Enemy Pooling
- Enemy Spawner

이번 작업에서는 여기에 코루틴 기반 연출을 추가했습니다.

### 추가한 코루틴 작업

| 구분 | 스크립트 | 목적 |
|---|---|---|
| Enemy 사망 연출 | `EnemyDeathEffect.cs` | 피격된 Enemy가 바로 사라지지 않고 사망 연출 후 Pool 반환 |
| 터렛 발사 반동 | `TurretFireEffect.cs` | 발사 시 포신이 뒤로 밀렸다가 원래 위치로 복귀 |
| 포신 과열 / 냉각 연출 | `TurretBarrelCoolingEffect.cs` | 발사 누적에 따라 포신 색상 변화 및 냉각 연출 |

---

## 2. Enemy 사망 연출 코루틴

### 목적

Enemy가 Projectile에 맞았을 때 바로 비활성화되지 않고, 짧은 사망 연출을 실행한 뒤 Pool로 반환되도록 구성했습니다.

### 관련 스크립트

```text
Enemy.cs
EnemyDeathEffect.cs
```

### 처리 흐름

```text
Projectile 충돌
→ Enemy.TakeHit()
→ Enemy 이동 중지
→ Enemy Collider 비활성화
→ EnemyDeathEffect.DeathEffectRoutine() 실행
→ Enemy Scale 감소
→ 사망 마커 생성
→ EnemyPool로 반환
```

### 역할 분리

| 스크립트 | 책임 |
|---|---|
| `Enemy.cs` | 피격 처리, 사망 상태 관리, Pool 반환 |
| `EnemyDeathEffect.cs` | 사망 연출 코루틴 담당 |

### 코루틴 적용 이유

사망 연출은 일정 시간 동안 순차적으로 진행되어야 하므로 코루틴이 적합합니다.

```text
사망 시작
→ 일정 시간 동안 크기 감소
→ 사망 위치 마커 생성
→ 잠시 대기
→ Pool 반환
```

이 방식은 `Update()`에서 상태를 계속 검사하는 방식보다 흐름을 읽기 쉽습니다.

---

## 3. EnemyDeathEffect.cs

### 주요 기능

- Enemy의 Scale을 점점 줄임
- 사망 위치에 임시 마커 생성
- 마커를 일정 시간 뒤 제거

### 핵심 구조

```csharp
public IEnumerator DeathEffectRoutine()
{
    float deathTimer = 0f;
    Vector3 startScale = transform.localScale;

    while (deathTimer < deathDuration)
    {
        deathTimer += Time.deltaTime;

        float ratio = deathTimer / deathDuration;
        ratio = Mathf.Clamp01(ratio);

        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ratio);

        yield return null;
    }

    transform.localScale = Vector3.zero;

    CreateDeathMarker();

    yield return new WaitForSeconds(deathMarkerLifeTimeSeconds);
}
```

### 구현 포인트

- `while` 반복문과 `yield return null`을 사용해 매 프레임 Scale을 갱신합니다.
- `startScale`을 따로 저장해 Lerp 기준값이 흔들리지 않도록 했습니다.
- Pooling 환경에서는 Enemy가 재사용되므로, Enemy 초기화 시 Scale을 원래 값으로 돌려야 합니다.

---

## 4. Enemy.cs 연결 방식

Enemy는 사망 연출을 직접 처리하지 않고, `EnemyDeathEffect` 컴포넌트에게 연출만 요청합니다.

### 권장 구조

```text
Enemy
├── Enemy.cs
├── EnemyDeathEffect.cs
└── Collider
```

### 핵심 흐름

```csharp
public void TakeHit()
{
    if (isDying)
    {
        return;
    }

    isDying = true;

    if (enemyCollider != null)
    {
        enemyCollider.enabled = false;
    }

    StartCoroutine(DeathRoutine());
}
```

```csharp
private IEnumerator DeathRoutine()
{
    if (deathEffect != null)
    {
        yield return deathEffect.DeathEffectRoutine();
    }

    ReturnToPool();
}
```

### 주의점

`IEnumerator` 함수는 일반 함수처럼 호출하면 실행되지 않습니다.

```csharp
DeathRoutine(); // 잘못된 방식
```

반드시 `StartCoroutine()`으로 실행해야 합니다.

```csharp
StartCoroutine(DeathRoutine()); // 올바른 방식
```

---

## 5. 터렛 발사 반동 코루틴

### 목적

Projectile 발사 시 포신이 살짝 뒤로 밀렸다가 원래 위치로 돌아오는 반동 연출을 추가했습니다.

### 관련 스크립트

```text
TurretShooter.cs
TurretFireEffect.cs
```

### 처리 흐름

```text
발사 조건 확인
→ FireRoutine() 시작
→ Projectile 발사
→ TurretFireEffect.FireEffectRoutine() 실행
→ 포신이 뒤로 이동
→ 포신이 원래 위치로 복귀
```

### 코루틴 적용 이유

발사 반동은 “짧은 시간 동안 위치를 변경하고 다시 되돌리는 연출”이므로 코루틴에 적합합니다.

---

## 6. TurretFireEffect.cs

### 주요 기능

- 포신 Transform을 뒤로 이동
- 일정 시간 동안 원래 위치로 복귀
- 반동 거리와 시간을 Inspector에서 조절 가능

### 권장 Inspector 값

| 항목 | 예시 값 |
|---|---:|
| `Recoil Distance` | `0.1 ~ 0.3` |
| `Recoil Back Duration` | `0.03` |
| `Recoil Return Duration` | `0.12` |

### 권장 연결

`Barrel Transform`에는 PitchPivot이 아니라 실제 포신 오브젝트를 연결합니다.

```text
TurretHead
└── TurretHeadPitchPivot
    └── GunBarrel      ← Barrel Transform
        └── MuzzlePoint
```

---

## 7. TurretShooter.cs 반동 연결

### 핵심 처리

`TurretShooter`는 발사 가능 여부를 확인하고, 발사 루틴을 코루틴으로 실행합니다.

```csharp
public void TickShoot()
{
    if (!CanShoot())
    {
        return;
    }

    StartCoroutine(FireRoutine());
}
```

### 중복 실행 방지

`TurretController`는 매 프레임 `TickShoot()`을 호출하므로, 코루틴이 중복 실행되지 않도록 `isFiring` 플래그를 사용했습니다.

```csharp
private bool isFiring = false;
```

```csharp
private bool CanShoot()
{
    if (isFiring) return false;
    if (isCooling) return false;

    // 나머지 발사 조건 검사
    return true;
}
```

### 발사 루틴

```csharp
private IEnumerator FireRoutine()
{
    isFiring = true;

    lastFireTime = Time.time;

    FireProjectile();

    if (fireEffect != null)
    {
        yield return fireEffect.FireEffectRoutine();
    }

    isFiring = false;
}
```

### 구현 포인트

- `lastFireTime`을 코루틴 시작 시점에 갱신합니다.
- 발사 반동은 Projectile 발사 후 실행하는 것이 자연스럽습니다.
- `isFiring`을 사용해 발사 루틴 중복 실행을 막습니다.

---

## 8. 포신 과열 / 냉각 코루틴

### 목적

일정 횟수 이상 발사하면 포신이 과열되고, 냉각되는 동안 발사가 제한되는 구조를 실험했습니다.

### 관련 스크립트

```text
TurretShooter.cs
TurretBarrelCoolingEffect.cs
```

### 의도한 흐름

```text
1발 발사
→ 포신 색상 약간 변화

2발 발사
→ 포신 색상 더 붉어짐

지정 발수 도달
→ 과열 상태 진입
→ 냉각 코루틴 실행
→ 냉각 중 발사 불가
→ 원래 색으로 복귀
→ 발사 카운트 초기화
```

---

## 9. 냉각 시스템 책임 분리

### TurretShooter.cs

`TurretShooter`는 과열과 발사 가능 여부를 관리합니다.

담당 책임:

- 현재 발사 횟수 관리
- 과열 기준 도달 여부 판단
- 냉각 중 발사 차단
- 냉각 코루틴 호출

### TurretBarrelCoolingEffect.cs

`TurretBarrelCoolingEffect`는 색상 연출만 담당합니다.

담당 책임:

- 발사 비율에 따라 포신 색상 변경
- 냉각 중 원래 색으로 복귀

---

## 10. 포신 냉각 시스템 구현 시 주의점

### 1. 상태 변수 이름 명확화

기존에 `complateCooling`처럼 의미가 애매한 이름을 쓰면 혼동이 생길 수 있습니다.

권장 이름:

```csharp
private bool isCooling;
private bool isOverHeated;
```

상태 관리는 `TurretShooter`에서 담당하는 것이 좋습니다.

---

### 2. Color 직접 비교 지양

아래 방식은 권장하지 않습니다.

```csharp
while (barrelRenderer.material.color != heatColor)
```

`Color`는 float 값이므로 직접 비교가 안정적이지 않습니다.

대신 발사 횟수 비율을 기준으로 색상을 결정하는 방식이 좋습니다.

```csharp
float heatRatio = currentFireCount / (float)overHeatingFireCount;
```

```csharp
Color targetColor = Color.Lerp(originalColor, heatColor, heatRatio);
```

---

### 3. 정수 나눗셈 주의

아래 코드는 의도대로 동작하지 않을 수 있습니다.

```csharp
float colorRatio = 1 / count;
```

`1`과 `count`가 모두 정수이므로 정수 나눗셈이 먼저 발생합니다.

수정:

```csharp
float colorRatio = 1f / count;
```

더 권장하는 방식:

```csharp
float heatRatio = currentFireCount / (float)overHeatingFireCount;
```

---

### 4. ProBuilder Material 이슈

GunBarrel을 ProBuilder로 만들었을 경우, 기본 Material이 아래처럼 설정될 수 있습니다.

```text
ProBuilderDefault
Shader: ProBuilder/Standard Vertex Color
```

이 경우 `Renderer.material.color` 변경이 화면에 잘 반영되지 않을 수 있습니다.

### 해결 방법

GunBarrel 전용 Material을 새로 만들어 적용합니다.

```text
Assets 우클릭
→ Create
→ Material
→ 이름: M_GunBarrel
```

Material 설정:

```text
Shader: Standard
또는
Shader: Universal Render Pipeline/Lit
```

그 후 GunBarrel의 Mesh Renderer에 적용합니다.

```text
GunBarrel
→ Mesh Renderer
→ Materials
→ Element 0
→ M_GunBarrel 연결
```

---

## 11. Unity Inspector 연결 요약

## 11.1 TurretShooter

| 필드 | 연결 |
|---|---|
| `Aiming Controller` | `TurretAimingController` |
| `Muzzle Point Transform` | `MuzzlePoint` |
| `Projectile Pool` | `ProjectilePool` |
| `Fire Effect` | `TurretFireEffect` |
| `Barrel Cooling Effect` | `TurretBarrelCoolingEffect` |

---

## 11.2 TurretFireEffect

| 필드 | 연결 |
|---|---|
| `Barrel Transform` | 실제 포신 오브젝트 `GunBarrel` |

---

## 11.3 TurretBarrelCoolingEffect

| 필드 | 연결 |
|---|---|
| `Barrel Obj` 또는 `Barrel Renderer` | `GunBarrel` |
| `Heat Color` | Red 계열 |
| `Cooling Duration` | 예: `1.0` |

---

## 11.4 Enemy

| 필드 | 연결 |
|---|---|
| `Death Effect` | 같은 오브젝트의 `EnemyDeathEffect` |

권장 구조:

```text
Enemy
├── Enemy.cs
├── EnemyDeathEffect.cs
└── Collider
```

---

## 12. 검증 체크리스트

### Enemy 사망 연출

- [ ] Projectile이 Enemy와 충돌하는가
- [ ] Enemy가 피격 후 이동을 멈추는가
- [ ] Collider가 비활성화되는가
- [ ] EnemyDeathEffect 코루틴이 실행되는가
- [ ] Scale이 서서히 줄어드는가
- [ ] 사망 마커가 생성되는가
- [ ] 연출 종료 후 Enemy가 Pool로 반환되는가

### 터렛 발사 반동

- [ ] 발사 시 Projectile이 정상 발사되는가
- [ ] 포신이 뒤로 밀리는가
- [ ] 포신이 원래 위치로 돌아오는가
- [ ] 발사 코루틴이 중복 실행되지 않는가
- [ ] `isFiring`이 정상적으로 발사 중복을 막는가

### 포신 냉각 연출

- [ ] 발사 횟수에 따라 포신 색상이 변화하는가
- [ ] 지정 발수 이후 냉각 상태에 들어가는가
- [ ] 냉각 중 발사가 막히는가
- [ ] 냉각 후 발사 카운트가 초기화되는가
- [ ] Material / Shader 설정 때문에 색상이 안 보이는 문제는 없는가

---

## 13. 이번 작업의 학습 포인트

이번 코루틴 작업을 통해 다음 내용을 학습했습니다.

- `IEnumerator` 기반 코루틴 작성
- `StartCoroutine()`을 통한 코루틴 실행
- `yield return null`을 이용한 매 프레임 연출 처리
- `yield return new WaitForSeconds()`를 이용한 시간 대기
- 코루틴 중복 실행 방지를 위한 상태 플래그 사용
- `Vector3.Lerp()`를 이용한 위치 / 크기 보간
- `Color.Lerp()`를 이용한 색상 보간
- Pooling 구조에서 연출 종료 후 반환 처리
- 발사 로직과 연출 로직의 책임 분리

---

## 14. 작업 상태 요약

### 완료한 작업

- Enemy 사망 연출 전용 스크립트 설계
- Enemy와 DeathEffect 연결 구조 정리
- 발사 반동 전용 스크립트 추가
- TurretShooter에서 발사 루틴 코루틴화
- 코루틴 중복 실행 방지 구조 추가
- 포신 과열 / 냉각 연출 구조 실험
- ProBuilder Material 색상 변경 이슈 확인

### 추가 개선 예정

- 포신 냉각 색상 연출 안정화
- GunBarrel 전용 Material 적용
- 냉각 시스템 상태 변수 정리
- Enemy 사망 연출과 Pool 반환 검증
- 반동 연출 수치 조정
- 필요 시 Muzzle Flash 이펙트 추가

---

## 15. 참고

이번 README는 코루틴 연출 추가 작업만 정리한 문서입니다.  
기존 터렛 조준, Pooling, Enemy Spawner 관련 내용은 별도 README 또는 이전 커밋 내용을 참고합니다.
