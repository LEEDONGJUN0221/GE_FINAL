using UnityEngine;
using System.Collections;

public class Stage3BossAttackController : MonoBehaviour
{
    [Header("SpriteRenderer (없으면 자동으로 찾음)")]
    public SpriteRenderer spriteRenderer;

    [Header("Apple Boss Sprites (choice2 = 0)")]
    public Sprite appleIdleSprite;
    public Sprite appleAttackSprite;

    [Header("Strawberry Boss Sprites (choice2 = 1)")]
    public Sprite strawberryIdleSprite;
    public Sprite strawberryAttackSprite;

    [Header("Fallback")]
    public bool defaultToStrawberryWhenUnset = true;

    // 선택에 따라 실제로 사용할 스프라이트
    private Sprite idleSprite;
    private Sprite attackSprite;

    private Coroutine attackRoutine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ApplyThemeFromRunData();
        SetIdle();
    }

    // 🔑 Stage2에서 고른 사과/딸기(choice2) 반영
    private void ApplyThemeFromRunData()
    {
        int choice = (RunData.I != null) ? RunData.I.choice2 : -1; // Stage2 선택

        if (choice == 0) // Apple
        {
            idleSprite = appleIdleSprite;
            attackSprite = appleAttackSprite;
        }
        else if (choice == 1) // Strawberry
        {
            idleSprite = strawberryIdleSprite;
            attackSprite = strawberryAttackSprite;
        }
        else
        {
            // 미선택/예외 경로 (-1 등)
            if (defaultToStrawberryWhenUnset)
            {
                idleSprite = strawberryIdleSprite;
                attackSprite = strawberryAttackSprite;
            }
            else
            {
                idleSprite = appleIdleSprite;
                attackSprite = appleAttackSprite;
            }

            Debug.LogWarning($"[Stage3BossAttackController] choice2 is unset/invalid: {choice}. Using fallback.");
        }
    }

    // ✅ telegraphTime 동안 Attack 표시하고 Idle로 복귀
    public void SetAttackForSeconds(float seconds)
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(CoAttackForSeconds(seconds));
    }

    public void SetIdleNow()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        SetIdle();
    }

    private IEnumerator CoAttackForSeconds(float seconds)
    {
        if (spriteRenderer != null && attackSprite != null)
            spriteRenderer.sprite = attackSprite;

        yield return new WaitForSeconds(seconds);

        SetIdle();
        attackRoutine = null;
    }

    private void SetIdle()
    {
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }
}
