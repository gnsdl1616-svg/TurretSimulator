using System.Collections;
using UnityEngine;

public class TurretBarrelCoolingEffect : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("냉각 연출에 쓰일 포신 Object")]
    [SerializeField] private GameObject barrelObj;
    [SerializeField] private Renderer barrelRenderer;
    [Tooltip("OverHeating")]
    [SerializeField] private Color heatColor = Color.red;
    /*[Tooltip("과열 되는데 걸리는 시간")]
    [SerializeField] private float heatingDuration = 0.1f;*/
    [Tooltip("원래 색으로 돌아오는데 걸리는 시간")]
    [SerializeField] private float coolingDuration = 1.0f;

    private Color originalColor;
    //private bool completeCooling = false;
    //public bool CompleteCooling => completeCooling;

    private void Awake()
    {
        if (barrelObj != null)
        {
            barrelRenderer = barrelObj.GetComponent<Renderer>();

            if (barrelRenderer != null)
            {
                originalColor = barrelRenderer.material.color;
            }
        }
    }

    public void SetHeatRatio(float heatRatio)
    {
        if (barrelRenderer == null)
        {
            return;
        }

        heatRatio = Mathf.Clamp01(heatRatio);

        barrelRenderer.material.color = Color.Lerp(
            originalColor,
            heatColor,
            heatRatio
            );
    }

    public IEnumerator CoolingEffectRoutine()
    {
        if (barrelRenderer == null)
        {
            yield break;
        }

        Color startColor = barrelRenderer.material.color;

        float elapsed = 0f;

        while (elapsed < coolingDuration)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / coolingDuration;
            ratio = Mathf.Clamp01(ratio);

            barrelRenderer.material.color = Color.Lerp(
                startColor,
                originalColor,
                ratio
                );

            yield return null;
        }

        barrelRenderer.material.color = originalColor;
    }
    /*public IEnumerator HeatingEffectRoutine(int count)
    {
        if (barrelObj == null)
        {
            yield break;
        }

        if (!completeCooling)
        {
            originalColor = barrelRanderer.material.color;
            completeCooling = true;
        }

        while (barrelRanderer.material.color != heatColor)
        {
            yield return CoolingBarrelRoutine(
                originalColor,
                heatColor,
                heatingDuration,
                count
                );
        }
    }

    public IEnumerator CoolingEffectRoutine()
    {
        int count = 1;
        if (barrelObj == null)
        {
            yield break;
        }

        if (completeCooling)
        {
            completeCooling = false;
        }

        {
            yield return CoolingBarrelRoutine(
                heatColor,
                originalColor,
                coolingDuration,
                count
                );
        }
    }

    private IEnumerator CoolingBarrelRoutine(Color from, Color to, float duration, int count)
    {
        if (duration <= 0f)
        {
            barrelRanderer.material.color = to;
            yield break;
        }

        float elapsed = 0f;
        float colorRatio = 1f / count;
        colorRatio = Mathf.Clamp01(colorRatio);
        to = to * colorRatio;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = elapsed / duration;
            ratio = Mathf.Clamp01(ratio);

            barrelRanderer.material.color = Color.Lerp(from, to, ratio);

            yield return null;
        }

        barrelRanderer.material.color = to;
    }*/
}
