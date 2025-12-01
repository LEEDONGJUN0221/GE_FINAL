using UnityEngine;
using System.Collections;

public class GridMovement : MonoBehaviour
{
    [Header("그리드 설정")]
<<<<<<< Updated upstream
    public Grid grid; 
    public float moveSpeed = 0.15f;     // 한 칸 이동하는 데 걸리는 시간 (초)

=======
    [Tooltip("씬에서 Grid 컴포넌트를 할당하세요. (Tilemap의 부모 객체)")]
    public Grid grid; 
    
    [Tooltip("이동 후 다음 입력을 받기까지의 딜레이 시간(초)")]
    public float moveDelay = 0.2f; 
    
>>>>>>> Stashed changes
    [Header("이동 경계 (Cell 좌표 기준)")]
    public Vector2Int minBounds = new Vector2Int(-10, -10);
    public Vector2Int maxBounds = new Vector2Int(10, 10);

    [Header("애니메이션 설정")]
<<<<<<< Updated upstream
    public Animator animator;
    public string idleStateName = "Princess_Idle";  // Idle 애니메이션 이름
    public string moveStateName = "Princess_Jump";  // Jump 애니메이션 이름

    private bool isMoving = false;
    private bool isAnimMoving = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float actualGridSize;
=======
    [Tooltip("플레이어 Animator를 넣어주세요.")]
    public Animator animator;
    [Tooltip("Animator에서 사용하는 이동 여부 Bool 파라미터 이름")]
    public string moveBoolName = "IsMoving";   // Animator 파라미터 이름

    private bool isMoving = false;
    private Rigidbody2D rb; 
    private float actualGridSize; 
    
    private SpriteRenderer spriteRenderer; 
>>>>>>> Stashed changes

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
<<<<<<< Updated upstream
        spriteRenderer = GetComponent<SpriteRenderer>();

=======
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
>>>>>>> Stashed changes
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;

        if (grid != null)
            actualGridSize = grid.cellSize.x;
<<<<<<< Updated upstream
=======
            Debug.Log($"Tilemap Grid Size가 {actualGridSize}로 설정되었습니다.");
        }
>>>>>>> Stashed changes
        else
        {
            actualGridSize = 1f;
            Debug.LogError("Grid 연결 안됨! 기본 1칸 크기로 이동합니다.");
        }

<<<<<<< Updated upstream
        PlayIdle(); // 시작할 때 Idle
=======
        // 시작할 때는 Idle이므로 false
        SetMoveAnimation(false);
>>>>>>> Stashed changes
    }

    void Update()
    {
        if (isMoving) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
<<<<<<< Updated upstream

        // 🔥 좌우 반전
        if (spriteRenderer != null)
        {
            if (h > 0) spriteRenderer.flipX = true;
            else if (h < 0) spriteRenderer.flipX = false;
=======
        
        // 방향에 따라 좌우 반전
        if (spriteRenderer != null)
        {
            if (h > 0)
                spriteRenderer.flipX = true;   // 오른쪽
            else if (h < 0)
                spriteRenderer.flipX = false;  // 왼쪽
>>>>>>> Stashed changes
        }

        // 🔥 대각 이동 금지
        if (h != 0 && v != 0)
            return;

        Vector3 moveDirection = Vector3.zero;

<<<<<<< Updated upstream
        if (h != 0) moveDirection = new Vector3(h, 0, 0);
        else if (v != 0) moveDirection = new Vector3(0, v, 0);
=======
        if (h != 0)
            moveDirection = new Vector3(h, 0, 0);
        else if (v != 0)
            moveDirection = new Vector3(0, v, 0);
>>>>>>> Stashed changes

        if (moveDirection != Vector3.zero)
        {
            StartCoroutine(MoveOneStep(moveDirection));
        }
        else
        {
<<<<<<< Updated upstream
            // 입력 없음 → Idle 유지
            PlayIdle();
=======
            // 입력이 전혀 없고, 이동도 안 하는 상태면 Idle 유지
            SetMoveAnimation(false);
>>>>>>> Stashed changes
        }
    }

    IEnumerator MoveOneStep(Vector3 direction)
    {
        isMoving = true;
<<<<<<< Updated upstream
        PlayMove();   // 🔥 이동 중 → Jump 애니메이션
=======
        SetMoveAnimation(true);   // 🔥 이동 시작 → Jump 애니메이션(이동 애니메이션) 재생
>>>>>>> Stashed changes

        Vector3 startPos = transform.position;
        Vector3 targetWorldPosition = startPos + direction * actualGridSize;

        // 월드 좌표 → 그리드 셀 좌표
        Vector3Int targetCell = grid.WorldToCell(targetWorldPosition);

<<<<<<< Updated upstream
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
=======
        // 3. 경계 확인
        if (targetCell.x < minBounds.x || targetCell.x > maxBounds.x || 
            targetCell.y < minBounds.y || targetCell.y > maxBounds.y)
        {
            isMoving = false;
            SetMoveAnimation(false);   // 이동 실패 → Idle
            Debug.Log("그리드 경계 밖이므로 이동할 수 없습니다.");
            yield break;
        }
        
        // 4. 실제 이동 (셀 중앙으로 스냅)
        transform.position = grid.GetCellCenterWorld(targetCell); 

        // 한 칸 이동한 동안만 jump 애니메이션 보여주고
        yield return new WaitForSeconds(moveDelay);

        isMoving = false;
        SetMoveAnimation(false);  // 이동 끝 → Idle
    }

    // Animator bool 제어 함수
    void SetMoveAnimation(bool moving)
    {
        if (animator != null && !string.IsNullOrEmpty(moveBoolName))
        {
            animator.SetBool(moveBoolName, moving);
        }
>>>>>>> Stashed changes
    }
}
