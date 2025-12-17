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

    // 현재 선택에 따라 실제로 사용할 스프라이트(자동 세팅됨)
    private Sprite idleSprite;
    private Sprite attackSprite;

    private Coroutine attackRoutine;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ApplyThemeFromRunData();
        SetIdle();
    }

    // ✅ Stage1 선택값에 따라 스프라이트 자동 선택
    private void ApplyThemeFromRunData()
    {
        // 기본값: Dark로 fallback
        int choice = (RunData.I != null) ? RunData.I.choice1 : 1; // Stage1 선택값

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

    // WarningManager에서 호출
    public void PlayAttack()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        if (spriteRenderer != null && attackSprite != null)
            spriteRenderer.sprite = attackSprite;

        yield return new WaitForSeconds(attackImageDuration);

        SetIdle();
        attackRoutine = null;
    }

    private void SetIdle()
    {
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }
}
