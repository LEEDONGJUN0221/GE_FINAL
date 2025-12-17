using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("References")]
    public GridMovement gridMovement;

    [Header("Move Delay (슬로우용)")]
    public float normalMoveDelay = 0.3f; // GridMovement 인스펙터 Move Delay와 동일
    public float slowMoveDelay = 0.6f;   // 슬로우 상태에서의 Move Delay
    public float slowDuration = 1.0f;    // 슬로우 유지 시간

    private int lastVinePatternHitId = -1;
    private Coroutine slowCoroutine;

    private void Start()
    {
        if (gridMovement != null)
        {
            gridMovement.moveDelay = normalMoveDelay;
        }
    }

    /// <summary>
    /// 가시에 맞았을 때 Stage4 GameManager에 데미지 보고
    /// </summary>
    public void ApplyVineDamage(int damage)
    {
        if (Stage4GameManager.Instance != null)
        {
            Stage4GameManager.Instance.TakeDamage(damage);
        }

        // TODO: 여기에서 피격 애니메이션, 빨간색 깜빡임 호출 예정
    }

    /// <summary>
    /// 슬로우 효과 적용 (중복 적용 시 타이머 리셋)
    /// </summary>
    public void ApplySlow()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(CoSlow());
    }

    private IEnumerator CoSlow()
    {
        if (gridMovement != null)
        {
            gridMovement.moveDelay = slowMoveDelay;
        }

        yield return new WaitForSeconds(slowDuration);

        if (gridMovement != null)
        {
            gridMovement.moveDelay = normalMoveDelay;
        }
    }

    public bool TryConsumeVineHit(int patternId)
    {
        if (patternId == lastVinePatternHitId)
            return false;

        lastVinePatternHitId = patternId;
        return true;
    }
}
