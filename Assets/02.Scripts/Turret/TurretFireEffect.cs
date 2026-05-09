using System.Collections;
using UnityEngine;

public class TurretFireEffect : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("반동 연출에 쓰일 포신 Transform")]
    [SerializeField] private Transform barrelTransform;

    [Header("Recoil")]
    [Tooltip("포신이 뒤로 밀리는 거리")]
    [SerializeField] private float recoilDistance = 0.2f;

    [Tooltip("뒤로 밀리는 데 걸리는 시간입니다.")]
    [SerializeField] private float recoilBackDuration = 0.03f;

    [Tooltip("원래 위치로 돌아오는 데 걸리는 시간입니다.")]
    [SerializeField] private float recoilReturnDuration = 0.12f;

    private Vector3 originalLocalPosition;
    private bool hasOriginalPosition = false;

    private void Awake()
    {
        if (barrelTransform != null)
        {
            originalLocalPosition = barrelTransform.localPosition;
            hasOriginalPosition = true;
        }
    }

    public IEnumerator FireEffectRoutine()
    {
        if (barrelTransform == null)
        {
            yield break;
        }

        if (!hasOriginalPosition)
        {
            originalLocalPosition = barrelTransform.localPosition;
            hasOriginalPosition = true;
        }

        Vector3 recoilLocalPosition = originalLocalPosition - Vector3.forward * recoilDistance;

        yield return MoveBarrelRoutine(
            originalLocalPosition,
            recoilLocalPosition,
            recoilBackDuration
            );

        yield return MoveBarrelRoutine(
            recoilLocalPosition,
            originalLocalPosition,
            recoilReturnDuration
            );

        barrelTransform.localPosition = originalLocalPosition;
        /*float firingRecoileTimer = 0;
        Vector3 originalPosition = barrelTransform.transform.localPosition;

        float barrelPivotPoint = originalPosition.z;

        float recoilePosition_Z = barrelPivotPoint - recoilDistance;

        Vector3 recoilePosition = new Vector3(originalPosition.x, originalPosition.y, recoilePosition_Z);

        barrelTransform.transform.localPosition = Vector3.Lerp(originalPosition, recoilePosition, 0.1f);

        yield return null;

        while (firingRecoileTimer < recoilReturnDuration)
        {
            firingRecoileTimer += Time.deltaTime;

            float ratio = firingRecoileTimer / recoilReturnDuration;
            ratio = Mathf.Clamp01(ratio);

            barrelTransform.transform.localPosition = Vector3.Lerp(recoilePosition, originalPosition, ratio);

            yield return null;
        }
        barrelTransform.transform.localPosition = originalPosition;*/
    }


    private IEnumerator MoveBarrelRoutine(Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            barrelTransform.localPosition = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / duration;
            ratio = Mathf.Clamp01(ratio);

            barrelTransform.localPosition = Vector3.Lerp(from, to, ratio);

            yield return null;
        }

        barrelTransform.localPosition = to;
    }
}
