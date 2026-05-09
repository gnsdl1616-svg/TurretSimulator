using System.Collections;
using UnityEngine;

public class EnemyDeathEffect : MonoBehaviour
{
    [SerializeField] private float deathDuration = 0.5f;
    [SerializeField] private float deathMarkerLifeTimeSeconds = 0.5f;

    [SerializeField]
    [Tooltip("비워두면 transform.position을 조준점으로 사용합니다.")]
    private Transform aimPoint;

    public Vector3 AimWorldPosition => aimPoint != null ? aimPoint.position : transform.position;

    public IEnumerator DeathEffectRoutine()
    {
        float deathTimer = 0f;
        Vector3 originalScale = transform.localScale;

        while (deathTimer < deathDuration)
        {
            deathTimer += Time.deltaTime;

            float ratio = deathTimer / deathDuration;
            ratio = Mathf.Clamp01(ratio);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, ratio);

            yield return null;
        }

        transform.localScale = Vector3.zero;

        CreateDeathMarker();

        // yield return new WaitForSeconds(deathMarkerLifeTimeSeconds);
    }

    private void CreateDeathMarker()
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "EnemyDeathMarker";
        marker.transform.position = AimWorldPosition;
        marker.transform.localScale = Vector3.one * 0.45f;

        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            Destroy(markerCollider);
        }

        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            // 사망 지점을 눈에 띄게 보여주기 위한 임시 색상입니다.
            markerRenderer.material.color = new Color(1f, 0.2f, 0.2f, 1f);
        }

        Destroy(marker, deathMarkerLifeTimeSeconds);
    }
}
