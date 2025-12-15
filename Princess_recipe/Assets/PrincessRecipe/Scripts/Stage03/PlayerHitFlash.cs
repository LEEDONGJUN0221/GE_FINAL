using UnityEngine;
using System.Collections;

public class PlayerHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public float flashDuration = 0.25f;   // 총 깜빡임 시간
    public float blinkInterval = 0.05f;   // 깜빡임 간격
    public Color flashColor = new Color(1f, 0.2f, 0.2f, 1f); // 붉은색

    [Header("Targets (비우면 자식 포함 SpriteRenderer 자동 수집)")]
    public SpriteRenderer[] targets;

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (targets == null || targets.Length == 0)
            targets = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void Flash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(CoFlash());
    }

    private IEnumerator CoFlash()
    {
        if (targets == null || targets.Length == 0) yield break;

        // 원래 색 저장
        Color[] original = new Color[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            original[i] = targets[i] != null ? targets[i].color : Color.white;

        float t = 0f;
        bool toggle = false;

        while (t < flashDuration)
        {
            t += blinkInterval;
            toggle = !toggle;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                targets[i].color = toggle ? flashColor : original[i];
            }

            yield return new WaitForSeconds(blinkInterval);
        }

        // 복구
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            targets[i].color = original[i];
        }

        flashRoutine = null;
    }
}
