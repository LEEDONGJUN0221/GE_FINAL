using UnityEngine;
using System.Collections;

public class GridMovement : MonoBehaviour
{
    [Header("그리드 설정")]
    [Tooltip("씬에서 Grid 컴포넌트를 할당하세요. (Tilemap의 부모 객체)")]
    public Grid grid; 
    
    [Tooltip("이동 후 다음 입력을 받기까지의 딜레이 시간(초)")]
    public float moveDelay = 0.2f; 
    
    [Header("이동 경계 (Cell 좌표 기준)")]
    public Vector2Int minBounds = new Vector2Int(-10, -10);
    public Vector2Int maxBounds = new Vector2Int(10, 10);

    [Header("애니메이션 설정")]
    [Tooltip("플레이어 Animator를 넣어주세요.")]
    public Animator animator;
    [Tooltip("Animator에서 사용하는 이동 여부 Bool 파라미터 이름")]
    public string moveBoolName = "IsMoving";   // Animator 파라미터 이름

    private bool isMoving = false;
    private Rigidbody2D rb; 
    private float actualGridSize; 
    
    private SpriteRenderer spriteRenderer; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (grid != null)
        {
            actualGridSize = grid.cellSize.x;
            Debug.Log($"Tilemap Grid Size가 {actualGridSize}로 설정되었습니다.");
        }
        else
        {
            actualGridSize = 1f; 
            Debug.LogError("Grid 컴포넌트가 할당되지 않았습니다. 기본 Grid Size (1.0f)를 사용합니다.");
        }

        // 시작할 때는 Idle이므로 false
        SetMoveAnimation(false);
    }
    
    void Update()
    {
        if (isMoving) return; 
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        // 방향에 따라 좌우 반전
        if (spriteRenderer != null)
        {
            if (h > 0)
                spriteRenderer.flipX = true;   // 오른쪽
            else if (h < 0)
                spriteRenderer.flipX = false;  // 왼쪽
        }

        // 대각선 이동 금지
        if (h != 0 && v != 0)
            return;

        Vector3 moveDirection = Vector3.zero;

        if (h != 0)
            moveDirection = new Vector3(h, 0, 0);
        else if (v != 0)
            moveDirection = new Vector3(0, v, 0);

        if (moveDirection != Vector3.zero)
        {
            StartCoroutine(MoveOneStep(moveDirection));
        }
        else
        {
            // 입력이 전혀 없고, 이동도 안 하는 상태면 Idle 유지
            SetMoveAnimation(false);
        }
    }

    IEnumerator MoveOneStep(Vector3 direction)
    {
        isMoving = true;
        SetMoveAnimation(true);   // 🔥 이동 시작 → Jump 애니메이션(이동 애니메이션) 재생

        // 1. 다음 목표 월드 위치 계산
        Vector3 targetWorldPosition = transform.position + direction * actualGridSize;

        // 2. 목표 월드 위치를 셀 좌표로 변환
        Vector3Int targetCell = grid.WorldToCell(targetWorldPosition);

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
    }
}
