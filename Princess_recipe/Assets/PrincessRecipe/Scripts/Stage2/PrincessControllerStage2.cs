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

    [Header("무적 / 깜빡임 설정")]
    [Tooltip("피격 후 무적 시간(초)")]
    public float invincibleDuration = 2f;

    [Tooltip("무적 중 깜빡임 간격(초)")]
    public float blinkInterval = 0.1f;

    [Tooltip("위험 블럭 태그 이름")]
    public string obstacleTag = "Stage2_Obstacle";

   


    private bool isMoving = false;
    private bool isInvincible = false;
    private Coroutine invincibleRoutine = null;

    private Vector3Int currentCell;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private WarningManagerStage2 warningManager;

    public Vector3Int CurrentCell => currentCell;

    // ===== 이동 입력 버퍼 (이동 중에 눌러도 1개 저장) =====
    private bool hasBufferedInput = false;
    private Vector3Int bufferedDelta = Vector3Int.zero;   // 다음 이동 예약(셀 기준)
    private Vector2 bufferedDir = Vector2.zero;           // 스프라이트 방향용

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

        warningManager = FindAnyObjectByType<WarningManagerStage2>();
        if (warningManager == null)
        {
            Debug.LogError("[PrincessControllerStage2] WarningManagerStage2를 찾지 못했습니다!");
        }


    }

    void Update()
    {
        if (grid == null) return;

        // =================================================
        // 1. 위험 블럭 위에 서 있는지 체크 (이동 여부 무관)
        // =================================================
        if (!isInvincible && warningManager != null)
        {
            if (warningManager.IsDangerCell(transform.position))
            {
                TakeBlockDamage();
            }
        }

        // =================================================
        // 2. 입력 받기 (GetKeyDown → 꾹 눌러도 1회 이동)
        //    이동 중이어도 입력은 "버퍼"에 저장한다
        // =================================================
        Vector3Int delta = Vector3Int.zero;
        Vector2 dir = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.D))
        {
            delta = Vector3Int.right;
            dir = Vector2.right;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            delta = Vector3Int.left;
            dir = Vector2.left;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            delta = Vector3Int.up;
            dir = Vector2.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            delta = Vector3Int.down;
            dir = Vector2.down;
        }
        else
        {
            return; // 아무 키도 안 눌렸으면 종료
        }

        // =================================================
        // 3. 이동 중이면 → 버퍼에 저장하고 끝
        // =================================================
        if (isMoving)
        {
            bufferedDelta = delta;
            bufferedDir = dir;
            hasBufferedInput = true;
            return;
        }

        // =================================================
        // 4. 이동 시도
        // =================================================
        TryMove(delta, dir);
    }


    void TryMove(Vector3Int delta, Vector2 dir)
    {
        Vector3Int nextCell = currentCell + delta;

        // 4. 스프라이트 방향
        if (spriteRenderer != null)
        {
            if (dir.x > 0) spriteRenderer.flipX = true;
            else if (dir.x < 0) spriteRenderer.flipX = false;
        }

        // 5. 이동 경계 체크
        if (nextCell.x < minBounds.x || nextCell.x > maxBounds.x ||
            nextCell.y < minBounds.y || nextCell.y > maxBounds.y)
        {
            return;
        }

        // 6. 이동 실행
        Vector3 targetPos = grid.GetCellCenterWorld(nextCell);
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
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 1️⃣ 셀 중앙으로 확정 스냅
        currentCell = nextCell;
        transform.position = grid.GetCellCenterWorld(currentCell);

        isMoving = false;
        // ===== 이동이 끝난 직후 버퍼 입력이 있으면 바로 1회 추가 이동 =====
        if (hasBufferedInput)
        {
            hasBufferedInput = false;
            Vector3Int d = bufferedDelta;
            Vector2 bd = bufferedDir;

            bufferedDelta = Vector3Int.zero;
            bufferedDir = Vector2.zero;

            // 다음 이동 바로 실행
            TryMove(d, bd);
        }

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
    



  public void TakeBlockDamage()
    {
        if (isInvincible) return;

        GameManagerStage2.Instance.TakeDamage(1);

        if (invincibleRoutine != null)
            StopCoroutine(invincibleRoutine);

        invincibleRoutine = StartCoroutine(InvincibleRoutine());
    }




}


