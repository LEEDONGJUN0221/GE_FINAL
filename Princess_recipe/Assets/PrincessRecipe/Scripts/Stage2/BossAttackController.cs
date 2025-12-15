using UnityEngine;
using System.Collections;

public class BossAttackController : MonoBehaviour
{
    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite attackSprite;

    [Header("Attack Timing")]
    public float attackImageDuration = 1.0f;

    [Header("Manager")]
    public WarningManagerStage2 warningManager;

    void Start()
    {
        // 기본 이미지 세팅
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    // WarningManager에서 호출할 함수
    public void PlayAttack()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        // 공격 이미지
        if (spriteRenderer != null && attackSprite != null)
            spriteRenderer.sprite = attackSprite;

        yield return new WaitForSeconds(attackImageDuration);

        // 다시 기본 이미지
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }
}
