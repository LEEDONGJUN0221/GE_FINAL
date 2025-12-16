using System.Collections;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;
    public Transform bossAnchor;
    public SpriteRenderer bossRenderer;

    [Header("Focus Offset (Boss is on top wall)")]
    [Tooltip("상단 벽면 보스가 화면에 너무 위로 치우치지 않도록 Y를 살짝 내립니다. (타일 1칸 정도: -1 추천)")]
    public float bossFocusOffsetY = -1.0f;

    [Header("Sizes")]
    [Tooltip("줌인 시 카메라 orthoSize (작을수록 더 확대)")]
    public float zoomInSize = 4.6f;

    [Tooltip("원래 게임뷰 orthoSize (현재 게임뷰: 6.5)")]
    public float zoomOutSize = 6.5f;

    [Header("Timing (Realtime)")]
    public float moveInDuration = 0.25f;
    public float holdDuration = 0.45f;
    public float moveOutDuration = 0.25f;

    [Header("Boss Color (Temp)")]
    public Color phase1Color = new Color(0.6f, 0.9f, 0.6f, 1f);
    public Color phase2Color = new Color(1.0f, 0.85f, 0.3f, 1f);
    public Color phase3Color = new Color(1.0f, 0.35f, 0.35f, 1f);
    public Color clearColor  = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("Shake (Realtime)")]
    public bool useShake = true;
    public float shakeDuration = 0.25f;
    public float shakeMagnitude = 0.12f;



    private Coroutine running;
    public bool IsPlaying => running != null;

    public void PlayBossCinematic(int phase)
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || bossAnchor == null) return;

        if (bossRenderer != null)
        {
            bossRenderer.color =
                (phase == 1) ? phase1Color :
                (phase == 2) ? phase2Color :
                (phase == 3) ? phase3Color : clearColor;
        }

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CoBossCinematic());
    }

    private IEnumerator CoBossCinematic()
    {
        // 0) 게임 정지
        float prevScale = Time.timeScale;
        Time.timeScale = 0f;

        // 원래 카메라 상태 캡쳐
        Vector3 originalPos = targetCamera.transform.position;
        float originalSize = targetCamera.orthographicSize;

        // 원래뷰 사이즈를 항상 6.5로 복귀시키고 싶다면 여기서 강제
        // (인스펙터가 6.5로 맞춰져 있다면 이 줄은 없어도 됨)
        zoomOutSize = 6.5f;

        // 보스 위치로 이동할 목표(카메라 z 유지)
        Vector3 bossPos = new Vector3(
            bossAnchor.position.x,
            bossAnchor.position.y + bossFocusOffsetY,
            originalPos.z
        );

        // 1) 이동+줌인
        yield return StartCoroutine(CoMoveAndZoomRealtime(
            originalPos, bossPos,
            originalSize, zoomInSize,
            moveInDuration
        ));

        // 2) 유지
        if (useShake)
            yield return StartCoroutine(CoShakeDuringHold(holdDuration, shakeDuration, shakeMagnitude));
        else
            yield return new WaitForSecondsRealtime(holdDuration);


        // 3) 원래로 이동+줌아웃
        yield return StartCoroutine(CoMoveAndZoomRealtime(
            bossPos, originalPos,
            zoomInSize, zoomOutSize,
            moveOutDuration
        ));

        // 4) 게임 재개
        Time.timeScale = prevScale;
        running = null;
    }

    private IEnumerator CoShakeDuringHold(float totalHold, float shakeDur, float mag)
    {
        float elapsed = 0f;
        Vector3 basePos = targetCamera.transform.position;

        // hold 전체 시간 동안 유지하되, 앞부분 shakeDur 만큼만 흔들고 나머지는 고정
        while (elapsed < totalHold)
        {
            elapsed += Time.unscaledDeltaTime;

            if (elapsed <= shakeDur)
            {
                float ox = Random.Range(-mag, mag);
                float oy = Random.Range(-mag, mag);
                targetCamera.transform.position = basePos + new Vector3(ox, oy, 0f);
            }
            else
            {
                targetCamera.transform.position = basePos;
            }

            yield return null;
        }

        targetCamera.transform.position = basePos;
    }


    private IEnumerator CoMoveAndZoomRealtime(
        Vector3 fromPos, Vector3 toPos,
        float fromSize, float toSize,
        float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            targetCamera.transform.position = Vector3.Lerp(fromPos, toPos, t);
            targetCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, t);
            yield return null;
        }

        targetCamera.transform.position = toPos;
        targetCamera.orthographicSize = toSize;
    }
}
