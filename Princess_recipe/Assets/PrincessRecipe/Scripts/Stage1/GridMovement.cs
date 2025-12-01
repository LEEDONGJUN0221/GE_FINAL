using UnityEngine;
using System.Collections;

public class GridMovement : MonoBehaviour
{
    [Header("그리드 설정")]
    public Grid grid; 
    public float moveSpeed = 0.15f;     // 한 칸 이동하는 데 걸리는 시간 (초)

    [Header("이동 경계 (Cell 좌표 기준)")]
    public Vector2Int minBounds = new Vector2Int(-10, -10);
    public Vector2Int maxBounds = new Vector2Int(10, 10);

    [Header("애니메이션 설정")]
    public Animator animator;
    public string idleStateName = "Princess_Idle";  // Idle 애니메이션 이름
    public string moveStateName = "Princess_Jump";  // Jump 애니메이션 이름

    private bool isMoving = false;
    private bool isAnimMoving = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float actualGridSize;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;

        if (grid != null)
            actualGridSize = grid.cellSize.x;
        else
        {
            actualGridSize = 1f;
            Debug.LogError("Grid 연결 안됨! 기본 1칸 크기로 이동합니다.");
        }

        PlayIdle(); // 시작할 때 Idle
    }

    void Update()
    {
        if (isMoving) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 🔥 좌우 반전
        if (spriteRenderer != null)
        {
            if (h > 0) spriteRenderer.flipX = true;
            else if (h < 0) spriteRenderer.flipX = false;
        }

        // 🔥 대각 이동 금지
        if (h != 0 && v != 0)
            return;

        Vector3 moveDirection = Vector3.zero;

        if (h != 0) moveDirection = new Vector3(h, 0, 0);
        else if (v != 0) moveDirection = new Vector3(0, v, 0);

        if (moveDirection != Vector3.zero)
        {
            StartCoroutine(MoveOneStep(moveDirection));
        }
        else
        {
            // 입력 없음 → Idle 유지
            PlayIdle();
        }
    }

    IEnumerator MoveOneStep(Vector3 direction)
    {
        isMoving = true;
        PlayMove();   // 🔥 이동 중 → Jump 애니메이션

        Vector3 startPos = transform.position;
        Vector3 targetWorldPosition = startPos + direction * actualGridSize;

        // 월드 좌표 → 그리드 셀 좌표
        Vector3Int targetCell = grid.WorldToCell(targetWorldPosition);

        // 🔥 경계 체크
        if (targetCell.x < minBounds.x || targetCell.x > maxBounds.x ||
            targetCell.y < minBounds.y || targetCell.y > maxBounds.y)
        {
            isMoving = false;
            PlayIdle();
            Debug.Log("경계 밖: 이동 불가");
            yield break;
        }

        // 목표 위치 = 셀 중앙
        Vector3 targetPos = grid.GetCellCenterWorld(targetCell);

        float elapsed = 0f;

        // 🔥 부드럽게 이동 (Lerp)
        while (elapsed < moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveSpeed);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 정확히 위치 보정
        transform.position = targetPos;

        isMoving = false;
        PlayIdle();
    }

    // -------------------------------
    // 애니메이션 함수
    // -------------------------------

    void PlayIdle()
    {
        if (animator == null) return;
        if (!isAnimMoving) return;  // 이미 Idle이면 패스

        animator.Play(idleStateName);
        isAnimMoving = false;
    }

    void PlayMove()
    {
        if (animator == null) return;
        if (isAnimMoving) return; // 이미 Jump(이동) 중이면 패스

        animator.Play(moveStateName);
        isAnimMoving = true;
    }
}
