using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reachDistance = 0.5f;

    [Header("Death")]
    [SerializeField] private EnemyDeathEffect deathEffect;

    private EnemyPool ownerPool;
    private Transform moveTarget;
    private Collider enemyCollider;

    private Vector3 originalScale;
    // private float deathTimer;
    private bool isDying = false;

    public bool IsDying => isDying;

    private void Awake()
    {
        enemyCollider = GetComponent<Collider>();
        originalScale = transform.localScale;

        if (deathEffect == null)
        {
            deathEffect = GetComponent<EnemyDeathEffect>();
        }
    }

    public void Init(EnemyPool pool, Transform target)
    {
        ownerPool = pool;
        moveTarget = target;

        isDying = false;
        // deathTimer = 0f;
        transform.localScale = originalScale;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
    }

    private void Update()
    {
        if (isDying)
        {
            //TickDeath();
            return;
        }

        MoveToTarget();
    }

    private void MoveToTarget()
    {
        if (moveTarget == null)
        {
            ReturnToPool();
            return;
        }

        Vector3 direction = moveTarget.position - transform.position;
        Vector3 directionZeroY = direction;
        directionZeroY.y = 0f;

        if (directionZeroY.sqrMagnitude <= reachDistance * reachDistance)
        {
            ReturnToPool();
            return;
        }

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            transform.forward = moveDirection;
        }
    }

    public void TakeHit()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        // deathTimer = 0f;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        if (deathEffect != null)
        {
            yield return deathEffect.DeathEffectRoutine();
        }

        ReturnToPool();
    }

    /*private void TickDeath()
    {
        deathTimer += Time.deltaTime;

        float ratio = deathTimer / deathDuration;
        ratio = Mathf.Clamp01(ratio);

        transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, ratio);

        if (ratio >= 1f)
        {
            ReturnToPool();
        }
    }*/

    private void ReturnToPool()
    {
        if (ownerPool != null)
        {
            ownerPool.ReturnEnemy(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
