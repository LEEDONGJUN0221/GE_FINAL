using UnityEngine;
using System.Collections;

// Grid 기반 셀 단위 이동 + 무적/깜빡임 + 벽 감지
public class PrincessControllerStage2 : MonoBehaviour
{
    [Header("그리드 설정")]
    [Tooltip("플레이어가 움직일 Grid (Tilemap의 부모)")]
    public Grid grid;
    [Tooltip("한 칸 이동에 걸리는 시간(초)")]
    public float moveTime = 0.15f;

    [Header("이동 경계 (Cell 좌표 기준)")]
    public Vector2Int minBounds = new Vector2Int(-10, -10);
    public Vector2Int maxBounds = new Vector2Int(10, 10);

    [Header("벽 / 장애물 설정")]
    [Tooltip("벽/블럭이 속한 레이어 (Stage2_Obstacle 등)")]
    public LayerMask obstacleLayer;
    [Tooltip("목적 셀 중심에서 이 반경 안에 장애물이 있으면 막힌 것으로 판단")]
    public float collisionRadius = 0.1f;

    [Header("무적 / 깜빡임 설정")]
    public float invincibleDuration = 3f;
    public float blinkInterval = 0.1f;
    [Tooltip("벽/블럭에 붙어 있는 태그 이름")]
    public string obstacleTag = "Stage2_Obstacle";

    private bool isMoving = false;
    private bool isInvincible = false;
    private Coroutine invincibleRoutine = null;

    private Vector3Int currentCell;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // 스프라이트 찾기
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (grid == null)
        {
            Debug.LogError("[PrincessControllerStage2] Grid가 설정되지 않았습니다!");
            return;
        }

        // 시작 위치를 그리드 셀 중앙에 스냅
        currentCell = grid.WorldToCell(transform.position);
        Vector3 center = grid.GetCellCenterWorld(currentCell);
        transform.position = center;

        // ➕ 콜라이더 자동 사이즈 조정 (그리드에 맞춰줌)
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Vector3 cellSize = grid.cellSize;
            // 셀보다 살짝 작은 0.8배 크기 → 모서리 끼임 방지
            box.size = new Vector2(cellSize.x * 0.8f, cellSize.y * 0.8f);
            box.offset = Vector2.zero;
        }

        Debug.Log("[PrincessControllerStage2] 시작 셀: " + currentCell);
    }

    void Update()
    {
        if (grid == null) return;
        if (isMoving) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 대각선 입력 방지
        if (h != 0 && v != 0)
            return;

        Vector3Int nextCell = currentCell;
        Vector2 dir = Vector2.zero;

        if (h > 0.1f)
        {
            nextCell += Vector3Int.right;
            dir = Vector2.right;
        }
        else if (h < -0.1f)
        {
            nextCell += Vector3Int.left;
            dir = Vector2.left;
        }
        else if (v > 0.1f)
        {
            nextCell += Vector3Int.up;
            dir = Vector2.up;
        }
        else if (v < -0.1f)
        {
            nextCell += Vector3Int.down;
            dir = Vector2.down;
        }

        if (dir == Vector2.zero)
            return;

        // 좌우 방향에 따라 스프라이트 반전
        if (spriteRenderer != null)
        {
            if (dir.x > 0.1f) spriteRenderer.flipX = true;
            else if (dir.x < -0.1f) spriteRenderer.flipX = false;
        }

        // 경계 체크
        if (nextCell.x < minBounds.x || nextCell.x > maxBounds.x ||
            nextCell.y < minBounds.y || nextCell.y > maxBounds.y)
        {
            Debug.Log("[PrincessControllerStage2] 경계 밖으로 이동 불가: " + nextCell);
            return;
        }

        // 목적 셀의 월드 중앙 좌표
        Vector3 targetPos = grid.GetCellCenterWorld(nextCell);

        // 이동하기 전에 그 셀에 장애물이 있는지 레이어로 체크
        Collider2D hit = Physics2D.OverlapCircle(targetPos, collisionRadius, obstacleLayer);
        if (hit != null)
        {
            Debug.Log($"[PrincessControllerStage2] 이동 취소 – 장애물({hit.name}) 때문에 막힘, 셀 {nextCell}");
            // 여기서 "부딪힌 느낌" 이펙트/사운드 재생해도 됨
            return;
        }

        // 실제 이동 코루틴 시작
        StartCoroutine(MoveToCell(nextCell, targetPos));
    }

    IEnumerator MoveToCell(Vector3Int nextCell, Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);

            if (rb != null)
                rb.MovePosition(newPos);
            else
                transform.position = newPos;

            yield return null;
        }

        if (rb != null)
            rb.MovePosition(targetPos);
        else
            transform.position = targetPos;

        currentCell = grid.WorldToCell(targetPos);
        isMoving = false;
    }

    // 벽/블럭(Trigger)에 닿았을 때 – 데미지 + 무적
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(obstacleTag))
            return;

        Debug.Log("[PrincessControllerStage2] OnTriggerEnter2D: " + other.name);

        if (isInvincible)
        {
            Debug.Log("[PrincessControllerStage2] 무적 상태라 데미지 무시");
            return;
        }

        GameManagerStage2.Instance.TakeDamage(1);
        Debug.Log("[PrincessControllerStage2] 데미지 1");

        if (invincibleRoutine != null)
            StopCoroutine(invincibleRoutine);

        invincibleRoutine = StartCoroutine(InvincibleRoutine());
    }

    IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibleDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            yield return new WaitForSeconds(blinkInterval);

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            yield return new WaitForSeconds(blinkInterval);

            elapsed += blinkInterval * 2f;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvincible = false;
        invincibleRoutine = null;
    }
}
