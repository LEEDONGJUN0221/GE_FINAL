using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    [Header("순찰 지점 설정")]
    public Vector2 point1;
    public Vector2 point2;

    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Vector2 currentTarget;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 현재 Y좌표 고정
        float y = transform.position.y;
        point1 = new Vector2(point1.x, y);
        point2 = new Vector2(point2.x, y);

        // 시작 시 가까운 지점 선택
        if (Vector2.Distance(transform.position, point1) < Vector2.Distance(transform.position, point2))
            currentTarget = point2;
        else
            currentTarget = point1;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        // 목표 y좌표 고정
        float y = transform.position.y;
        Vector2 target = new Vector2(currentTarget.x, y);

        // 이동
        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        // 도달 체크
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            currentTarget = (currentTarget.x == point1.x) ? point2 : point1;
        }

        UpdateSpriteDirection();
    }

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        float dirX = currentTarget.x - transform.position.x;
        if (dirX > 0.01f) spriteRenderer.flipX = false;
        else if (dirX < -0.01f) spriteRenderer.flipX = true;
    }

    public void IncreaseSpeed(float speedIncreaseAmount)
    {
        moveSpeed += speedIncreaseAmount;
        Debug.Log($"{gameObject.name}의 속도 증가! 현재 속도: {moveSpeed}");
    }

    public void StopMonster()
    {
        moveSpeed = 0f;
        Debug.Log($"{gameObject.name} 몬스터가 멈췄습니다.");
    }
}
