using UnityEngine;
using System.Collections;

public class BossAttackController : MonoBehaviour
{
    [Header("SpriteRenderer (없으면 자동으로 찾음)")]
    public SpriteRenderer spriteRenderer;

    [Header("White Choco Sprites")]
    public Sprite whiteIdleSprite;
    public Sprite whiteAttackSprite;

    [Header("Dark Choco Sprites")]
    public Sprite darkIdleSprite;
    public Sprite darkAttackSprite;

    [Header("Attack Timing")]
    public float attackImageDuration = 1.0f;

    // 현재 선택에 따라 실제로 사용할 스프라이트
    private Sprite idleSprite;
    private Sprite attackSprite;

    private Coroutine attackRoutine;

    void Awake()
    {
        // SpriteRenderer 자동 연결
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Stage1 선택값 기준으로 보스 테마 결정
        ApplyThemeFromRunData();

        // 시작 시 기본 이미지
        SetIdle();
    }

    // 🔑 Stage1에서 고른 초콜릿 타입 반영
    private void ApplyThemeFromRunData()
    {
        // 기본값은 Dark (안전장치)
        int choice = (RunData.I != null) ? RunData.I.choice1 : 1;

        bool isWhite = (choice == 0);

        if (isWhite)
        {
            idleSprite = whiteIdleSprite;
            attackSprite = whiteAttackSprite;
        }
        else
        {
            idleSprite = darkIdleSprite;
            attackSprite = darkAttackSprite;
        }
    }

    // ⚔️ WarningManagerStage2에서 호출
    public void PlayAttack()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        // 공격 이미지
        if (spriteRenderer != null && attackSprite != null)
            spriteRenderer.sprite = attackSprite;

        yield return new WaitForSeconds(attackImageDuration);

        // 다시 기본 이미지
        SetIdle();
        attackRoutine = null;
    }

    private void SetIdle()
    {
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }
}
