using System.Collections.Generic;
using UnityEngine;

public class VineDamageZone : MonoBehaviour
{
    [Header("Damage")]
    public int damagePerHit = 1;
    
    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip hurtClip;

    [Header("Optional Effects")]
    public bool applySlow = false;

    // 스포너가 주입해주는 "현재 패턴(턴) ID"
    private int patternId = -1;

    // "패턴당 1회"를 위해: 플레이어 instanceID -> 마지막으로 맞은 patternId
    private static readonly Dictionary<int, int> playerLastHitPattern = new Dictionary<int, int>();

    public void SetPatternId(int id)
    {
        patternId = id;
    }

    private void Awake()
    {
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamageOncePerPattern(other);
        // Debug.Log("[VineDamageZone] ENTER with: " + other.name + " tag=" + other.tag);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // GridMovement가 "한 칸 순간이동/스냅"처럼 들어오는 경우,
        // Enter를 놓칠 수 있어 Stay에서도 한 번 체크하되,
        // 아래 로직이 "패턴당 1회"로 막아줌.
        TryDamageOncePerPattern(other);
    }

    private void TryDamageOncePerPattern(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        // 패턴ID 주입이 안 된 상태면, "패턴당 1회" 보장이 불가하므로 데미지 스킵
        if (patternId < 0) return;

        int pid = other.gameObject.GetInstanceID();

        // 동일 패턴에서 이미 맞았으면 종료
        if (playerLastHitPattern.TryGetValue(pid, out int last) && last == patternId)
            return;

        // 기록(이번 패턴에 대해 1회 처리됨)
        playerLastHitPattern[pid] = patternId;

        // 실제 데미지
        if (Stage4GameManager.Instance != null)
            Stage4GameManager.Instance.TakeDamage(damagePerHit);

        if (sfxSource != null && hurtClip != null)
            sfxSource.PlayOneShot(hurtClip);

        // 부가효과(있으면 동작, 없으면 무시)
        if (applySlow)
            other.gameObject.SendMessage("ApplySlow", SendMessageOptions.DontRequireReceiver);

        other.gameObject.SendMessage("Flash", SendMessageOptions.DontRequireReceiver);
    }
}
