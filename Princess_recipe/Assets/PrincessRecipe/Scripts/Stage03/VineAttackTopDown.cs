using System.Collections;
using UnityEngine;

public class VineAttackTopDown : MonoBehaviour
{
    [Header("길이 / 속도 설정")]
    public float maxLength = 4f;      // 최대로 뻗는 길이 (배율 기준)
    public float growSpeed = 10f;     // 자라는 속도
    public float stayTime = 0.15f;    // 최대 길이 유지 시간
    public float shrinkSpeed = 12f;   // 줄어드는 속도

    [Header("공격 방향 (정규화 벡터)")]
    public Vector2 direction = Vector2.right;

    private BoxCollider2D col;
    private float baseX;   // 원래 X 스케일
    private float baseY;   // 원래 Y 스케일

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning("VineAttackTopDown: BoxCollider2D가 없습니다.", this);
        }

        baseX = transform.localScale.x;
        baseY = transform.localScale.y;
    }

    void OnEnable()
    {
        StartCoroutine(GrowRoutine());
    }

    // 스포너에서 방향 넘겨줌
    public void SetDirection(Vector2 dir)
    {
        if (dir == Vector2.zero)
            dir = Vector2.right;

        direction = dir.normalized;

        // 스프라이트가 위(0,1)를 보도록 만들어져 있다고 가정하고 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    IEnumerator GrowRoutine()
    {
        if (col != null)
            col.enabled = false;

        float currentLength = 0.01f;

        // 자라기
        while (currentLength < maxLength)
        {
            currentLength += growSpeed * Time.deltaTime;
            SetLength(currentLength);
            yield return null;
        }

        currentLength = maxLength;
        SetLength(currentLength);

        if (col != null)
            col.enabled = true;

        // 유지
        yield return new WaitForSeconds(stayTime);

        // 줄어들기
        while (currentLength > 0.01f)
        {
            currentLength -= shrinkSpeed * Time.deltaTime;
            SetLength(currentLength);
            yield return null;
        }

        // 다 끝나면 인스턴스 삭제 (프리팹은 안 없어진다!)
        Destroy(gameObject);
    }

void SetLength(float length)
{
    // Y 방향(위로)으로만 쭉 늘리기
    transform.localScale = new Vector3(baseX, baseY * length, 1f);

    // ⚠️ BoxCollider2D는 여기서 절대 건들지 말기!
}


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // collision.GetComponent<EnemyHealth>()?.TakeDamage(1);
        }
    }
}
