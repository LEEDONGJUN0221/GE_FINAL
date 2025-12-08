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
    public float dustOffsetY = -0.1f;

    [Header("사운드 설정")]
    [Tooltip("발자국 / 이동 효과음 클립")]
    public AudioClip moveSound;      // ★ 추가
    private AudioSource audioSource; // ★ 추가

    private bool isMoving = false;
    private Rigidbody2D rb;
    private float actualGridSize = 1f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ★ AudioSource 자동 연결 (없으면 자동 추가)
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

        if (spriteRenderer != null)
        {
            if (h > 0) spriteRenderer.flipX = true;
            else if (h < 0) spriteRenderer.flipX = false;
        }

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

        if (targetCell.x < minBounds.x || targetCell.x > maxBounds.x ||
            targetCell.y < minBounds.y || targetCell.y > maxBounds.y)
        {
            SetMoveAnimation(false);
            isMoving = false;
            yield break;
        }

        Vector3 endPos = grid.GetCellCenterWorld(targetCell);

        if (IsBlockedByObstacle(endPos))
        {
            SetMoveAnimation(false);
            isMoving = false;
            yield break;
        }

        // 먼지 이펙트
        SpawnDustAtPosition(startPos);

        // ★ 이동 효과음 재생
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

    void SpawnDustAtPosition(Vector3 worldPos)
    {
        if (dustPrefab == null || grid == null)
            return;

        Vector3Int cell = grid.WorldToCell(worldPos);
        Vector3 center = grid.GetCellCenterWorld(cell);
        center.y += dustOffsetY;

        GameObject dust = Instantiate(dustPrefab, center, Quaternion.identity);
        if (dustLifetime > 0f)
            Destroy(dust, dustLifetime);
    }
}
