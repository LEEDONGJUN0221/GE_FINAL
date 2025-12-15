using UnityEngine;
using System.Collections;

public class VineDamageZone : MonoBehaviour
{
    public int damagePerHit = 1;
    public bool applySlow = true;

    [Header("Overlap Check (Spawn 시 겹침 보정)")]
    public LayerMask playerLayer;          // 비워두면 Tag로만 체크
    public bool checkOverlapOnEnable = true;

    private int patternId = -1;
    private bool hasDamaged = false;
    private Collider2D myCol;

    public void SetPatternId(int newPatternId)
    {
        patternId = newPatternId;
        hasDamaged = false;
    }

    private void Awake()
    {
        myCol = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // 가시가 "겹친 상태로 생성"되는 경우 Enter가 안 찍힐 수 있어서 보정
        if (checkOverlapOnEnable)
            StartCoroutine(CoOverlapCheckNextFixed());
    }

    private IEnumerator CoOverlapCheckNextFixed()
    {
        yield return new WaitForFixedUpdate();

        if (hasDamaged) yield break;
        if (myCol == null) yield break;

        // 1) 레이어로 검사 가능하면 레이어로 (권장)
        if (playerLayer.value != 0)
        {
            Collider2D hit = Physics2D.OverlapBox(myCol.bounds.center, myCol.bounds.size, 0f, playerLayer);
            if (hit != null)
            {
                TryDamage(hit);
                yield break;
            }
        }

        // 2) 레이어를 안 쓰면 태그로 검사 (간단하지만 느림)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D pc = player.GetComponent<Collider2D>();
            if (pc != null && myCol.bounds.Intersects(pc.bounds))
            {
                TryDamage(pc);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (hasDamaged) return;
        if (!other.CompareTag("Player")) return;

        if (Stage4GameManager.Instance != null)
            Stage4GameManager.Instance.TakeDamage(damagePerHit);

        if (applySlow)
        {
            PlayerStatus st = other.GetComponent<PlayerStatus>();
            if (st != null) st.ApplySlow();
        }

        hasDamaged = true; // 패턴당 1회
        PlayerHitFlash flash = other.GetComponent<PlayerHitFlash>();
        if (flash != null) flash.Flash();


    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // OverlapBox 디버그 시각화
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
#endif
}
