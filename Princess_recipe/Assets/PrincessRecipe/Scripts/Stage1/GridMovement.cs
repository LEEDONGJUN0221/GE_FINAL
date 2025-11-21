using UnityEngine;
using System.Collections;

public class GridMovement : MonoBehaviour
{
    [Header("그리드 설정")]
    [Tooltip("씬에서 Grid 컴포넌트를 할당하세요. (Tilemap의 부모 객체)")]
    public Grid grid; // Tilemap의 Cell Size 정보를 가져오기 위한 Grid 컴포넌트 참조
    
    [Tooltip("이동 후 다음 입력을 받기까지의 딜레이 시간(초)")]
    public float moveDelay = 0.2f; 
    
    // --- 새로 추가된 경계 설정 변수 ---
    [Header("이동 경계 (Cell 좌표 기준)")]
    [Tooltip("캐릭터가 이동 가능한 최소 셀 좌표 (예: X=-5, Y=-3)")]
    public Vector2Int minBounds = new Vector2Int(-10, -10);
    [Tooltip("캐릭터가 이동 가능한 최대 셀 좌표 (예: X=5, Y=3)")]
    public Vector2Int maxBounds = new Vector2Int(10, 10);
    
    // ------------------------------------

    private bool isMoving = false;
    private Rigidbody2D rb; 
    private float actualGridSize; 
    
    // 🌟 추가: Sprite Renderer 컴포넌트 참조 변수
    private SpriteRenderer spriteRenderer; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 🌟 추가: SpriteRenderer 컴포넌트를 가져옵니다.
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (grid != null)
        {
            actualGridSize = grid.cellSize.x;
            Debug.Log($"Tilemap Grid Size가 {actualGridSize}로 설정되었습니다.");

            // 🌟 이전에 추가했다면 이 초기 위치 보정 코드를 제거하거나 주석 처리하세요. 🌟
            // Vector3Int startCell = grid.WorldToCell(transform.position);
            // transform.position = grid.GetCellCenterWorld(startCell);
        }
        else
        {
            actualGridSize = 1f; 
            Debug.LogError("Grid 컴포넌트가 할당되지 않았습니다. 기본 Grid Size (1.0f)를 사용합니다.");
        }
    }
    
    void Update()
    {
        if (isMoving) return; 
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        // 🌟 추가: 방향 전환 (Flip) 로직
        if (spriteRenderer != null)
        {
            if (h > 0)
            {
                // 오른쪽으로 이동: Flip을 끕니다. (정방향)
                spriteRenderer.flipX = true;
            }
            else if (h < 0)
            {
                // 왼쪽으로 이동: Flip을 켬으로써 캐릭터를 반전시킵니다.
                spriteRenderer.flipX = false;
            }
        }
        // -----------------------

        // 대각선 이동 금지
        if (h != 0 && v != 0)
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (h != 0)
        {
            moveDirection = new Vector3(h, 0, 0);
        }
        else if (v != 0)
        {
            moveDirection = new Vector3(0, v, 0);
        }

        if (moveDirection != Vector3.zero)
        {
            StartCoroutine(MoveOneStep(moveDirection));
        }
    }

    IEnumerator MoveOneStep(Vector3 direction)
    {
        isMoving = true;

        // 1. 다음 목표 월드 위치 계산
        Vector3 targetWorldPosition = transform.position + direction * actualGridSize;

        // 2. 목표 월드 위치를 셀 좌표로 변환
        Vector3Int targetCell = grid.WorldToCell(targetWorldPosition);

        // 3. --- 경계 확인 로직 ---
        if (targetCell.x < minBounds.x || targetCell.x > maxBounds.x || 
            targetCell.y < minBounds.y || targetCell.y > maxBounds.y)
        {
            // 경계를 벗어났다면, 이동하지 않고 즉시 코루틴 종료
            isMoving = false;
            Debug.Log("그리드 경계 밖이므로 이동할 수 없습니다.");
            yield break; // 코루틴을 여기서 중단합니다.
        }
        // -----------------------
        
        // 4. 경계 내에 있다면 이동 실행 (실제 이동 시에는 셀 중앙으로 보정하는 것이 좋습니다)
        // 캐릭터 위치를 목표 셀의 중앙으로 이동시킵니다.
        transform.position = grid.GetCellCenterWorld(targetCell); 

        yield return new WaitForSeconds(moveDelay);

        isMoving = false;
    }
}