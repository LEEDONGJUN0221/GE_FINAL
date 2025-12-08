using UnityEngine;
using System.Collections;

public class GridMovement : MonoBehaviour
{
    [Header("그리드 설정")]
    public Grid grid;

    [Tooltip("한 칸 이동 후 다음 입력 가능까지의 딜레이 시간(초)")]
    public float moveDelay = 0.2f;

    [Header("이동 경계 (Cell 좌표 기준)")]
    public Vector2Int minBounds = new Vector2Int(-10, -10);
    public Vector2Int maxBounds = new Vector2Int(10, 10);

    [Header("장애물(울타리) 설정")]
    public LayerMask obstacleLayer;
    public float obstacleCheckRadius = 0.1f;

    [Header("애니메이션 설정")]
    public Animator animator;
    public string moveBoolName = "IsMoving";

    [Header("이펙트 설정")]
    public GameObject dustPrefab;
    public float dustLifetime = 0.5f;

    [Tooltip("왼쪽으로 이동할 때 먼지 위치 오프셋")]
    public Vector2 dustOffsetLeft = new Vector2(-0.1f, -0.1f);
    [Tooltip("오른쪽으로 이동할 때 먼지 위치 오프셋")]
    public Vector2 dustOffsetRight = new Vector2(0.1f, -0.1f);
    [Tooltip("위로 이동할 때 먼지 위치 오프셋")]
    public Vector2 dustOffsetUp = new Vector2(0f, -0.05f);
    [Tooltip("아래로 이동할 때 먼지 위치 오프셋")]
    public Vector2 dustOffsetDown = new Vector2(0f, -0.15f);

    [Header("사운드 설정")]
    [Tooltip("발자국 / 이동 효과음 클립")]
    public AudioClip moveSound;
    private AudioSource audioSource;

    private bool isMoving = false;
    private Rigidbody2D rb;
    private float actualGridSize = 1f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;

        if (grid != null)
            actualGridSize = grid.cellSize.x;
        else
            actualGridSize = 1f;

        SetMoveAnimation(false);
    }

    void Update()
    {
        if (isMoving) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 플레이어 좌우 flip
        if (spriteRenderer != null)
        {
            if (h > 0) spriteRenderer.flipX = true;
            else if (h < 0) spriteRenderer.flipX = false;
        }

        // 대각선 이동 금지
        if (h != 0 && v != 0)
            return;

        Vector3 moveDirection = Vector3.zero;
        if (h != 0) moveDirection = new Vector3(h, 0, 0);
        else if (v != 0) moveDirection = new Vector3(0, v, 0);

        if (moveDirection != Vector3.zero)
            StartCoroutine(MoveOneStep(moveDirection));
        else
            SetMoveAnimation(false);
    }

    IEnumerator MoveOneStep(Vector3 direction)
    {
        isMoving = true;
        SetMoveAnimation(true);

        Vector3 startPos = transform.position;
        Vector3 targetWorld = startPos + direction * actualGridSize;
        Vector3Int targetCell = grid.WorldToCell(targetWorld);

        // 경계 체크
        if (targetCell.x < minBounds.x || targetCell.x > maxBounds.x ||
            targetCell.y < minBounds.y || targetCell.y > maxBounds.y)
        {
            SetMoveAnimation(false);
            isMoving = false;
            yield break;
        }

        Vector3 endPos = grid.GetCellCenterWorld(targetCell);

        // 장애물 체크
        if (IsBlockedByObstacle(endPos))
        {
            SetMoveAnimation(false);
            isMoving = false;
            yield break;
        }

        // 먼지 이펙트 (방향 정보 넘김)
        SpawnDustAtPosition(startPos, direction);

        // 이동 효과음
        if (moveSound != null)
            audioSource.PlayOneShot(moveSound);

        // 순간 이동
        transform.position = endPos;
        SetMoveAnimation(false);

        yield return new WaitForSeconds(moveDelay);
        isMoving = false;
    }

    bool IsBlockedByObstacle(Vector3 targetPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(targetPos, obstacleCheckRadius, obstacleLayer);
        return hit != null;
    }

    void SetMoveAnimation(bool moving)
    {
        if (animator != null && !string.IsNullOrEmpty(moveBoolName))
            animator.SetBool(moveBoolName, moving);
    }

    // 방향 기반으로 오프셋/flip 적용
    void SpawnDustAtPosition(Vector3 worldPos, Vector3 direction)
    {
        if (dustPrefab == null || grid == null) return;

        // 기본 위치: 떠난 칸의 셀 중앙
        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector3 center = grid.GetCellCenterWorld(cell);

        // 어떤 방향으로 이동했는지에 따라 오프셋 결정
        Vector2 offset = Vector2.zero;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                offset = dustOffsetRight;
            else if (direction.x < 0)
                offset = dustOffsetLeft;
        }
        else
        {
            if (direction.y > 0)
                offset = dustOffsetUp;
            else if (direction.y < 0)
                offset = dustOffsetDown;
        }

        center += new Vector3(offset.x, offset.y, 0f);

        GameObject dust = Instantiate(dustPrefab, center, Quaternion.identity);

        // ★ flip 처리 (좌우 + 상하)
        SpriteRenderer sr = dust.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // 좌우 flip
            if (direction.x > 0) sr.flipX = true;
            else if (direction.x < 0) sr.flipX = false;

            // 상하 flip (요청사항)
            if (direction.y > 0) sr.flipY = true;       // 위로 이동 → flipY
            else if (direction.y < 0) sr.flipY = false; // 아래 → 기본
        }

        if (dustLifetime > 0f)
            Destroy(dust, dustLifetime);
    }

}
