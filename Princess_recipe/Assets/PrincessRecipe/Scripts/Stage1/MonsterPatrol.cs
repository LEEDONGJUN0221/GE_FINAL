using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    [Header("순찰 지점 설정")]
    public Vector2 point1;
    public Vector2 point2;

    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("사망 스프라이트")]
    public Sprite deadSprite;

    private Vector2 currentTarget;
    private SpriteRenderer spriteRenderer;
    private Animator animator;          // ✅ 추가
    private Animation legacyAnim;       // ✅ 혹시 레거시면 이것도
    private bool isDead = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        legacyAnim = GetComponent<Animation>();
    }

    void Start()
    {
        float y = transform.position.y;
        point1 = new Vector2(point1.x, y);
        point2 = new Vector2(point2.x, y);

        currentTarget =
            Vector2.Distance(transform.position, point1) <
            Vector2.Distance(transform.position, point2)
                ? point2 : point1;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (isDead) return;

        float y = transform.position.y;
        Vector2 target = new Vector2(currentTarget.x, y);

        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target) < 0.1f)
            currentTarget = (currentTarget.x == point1.x) ? point2 : point1;

        UpdateSpriteDirection();
    }

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null || isDead) return;

        float dirX = currentTarget.x - transform.position.x;
        spriteRenderer.flipX = dirX < 0;
    }

    public void IncreaseSpeed(float amount)
    {
        if (isDead) return;
        moveSpeed += amount;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // ✅ 애니메이션이 스프라이트 덮어쓰는 걸 먼저 차단
        if (animator != null) animator.enabled = false;
        if (legacyAnim != null) legacyAnim.enabled = false;

        moveSpeed = 0f;

        if (spriteRenderer != null)
        {
            if (deadSprite != null) spriteRenderer.sprite = deadSprite;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
