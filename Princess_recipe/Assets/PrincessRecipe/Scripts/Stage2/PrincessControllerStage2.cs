using UnityEngine;





// 체스판 한 칸씩 이동 + 입력 버퍼 처리
public class PrincessControllerStage2 : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 12f;     // 한 칸 이동 속도 (조금 빠르게 해두면 답답함 줄어듦)
    public float stepSize = 1f;       // 한 칸 크기 (Grid 셀 크기와 동일하게)

    [Header("충돌 설정")]
    public LayerMask obstacleLayer;     // 벽/장애물 레이어
    public float collisionRadius = 0.3f; // 목적지 체크 반경

    private bool isMoving = false;    // 현재 이동 중인지
    private Vector3 targetPos;        // 이번에 도착해야 할 목표 위치

    // 이동 중에 눌린 키를 한 번 저장해두는 버퍼
    private bool hasBufferedInput = false;
    private Vector2 bufferedDir = Vector2.zero;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        targetPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log("[PrincessControllerStage2] 시작 위치: " + targetPos);
    }

    void Update()
    {
        // 항상 키 입력은 체크한다 (이동 중일 땐 버퍼에 저장)
        Vector2 dir = Vector2.zero;
        bool pressed = false;

        if (Input.GetKeyDown(KeyCode.W))
        {
            dir = Vector2.up;
            pressed = true;
            Debug.Log("W 입력: 위로 한 칸");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            dir = Vector2.down;
            pressed = true;
            Debug.Log("S 입력: 아래로 한 칸");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            dir = Vector2.left;
            pressed = true;
            Debug.Log("A 입력: 왼쪽으로 한 칸");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            dir = Vector2.right;
            pressed = true;
            Debug.Log("D 입력: 오른쪽으로 한 칸");
        }

        if (pressed)
        {
            if (!isMoving)
            {
                // 이동 중이 아니면 바로 이동 시작
                StartMove(dir);
            }
            else
            {
                // 이동 중이면 입력을 버퍼에 저장 (마지막 입력 1개만)
                bufferedDir = dir;
                hasBufferedInput = true;
                Debug.Log("[PrincessControllerStage2] 이동 중 입력 → 버퍼에 저장됨: " + bufferedDir);
            }
        }

        // 이동 진행은 FixedUpdate에서 함
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Stage2_Obstacle"))
        {
            GameManagerStage2.Instance.TakeDamage(1);
        }
    }


    void FixedUpdate()
    {
        if (!isMoving)
            return;

        // 목표 위치까지 부드럽게 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );

        // 목표 위치에 거의 도착했다면
        if (Vector3.Distance(transform.position, targetPos) < 0.001f)
        {
            transform.position = targetPos;
            isMoving = false;
            Debug.Log("[PrincessControllerStage2] 이동 완료, 현재 위치: " + transform.position);

            // 이동이 끝난 직후, 버퍼에 입력이 있으면 바로 다음 이동 시작
            if (hasBufferedInput)
            {
                Vector2 dir = bufferedDir;
                hasBufferedInput = false;   // 버퍼 비우기
                StartMove(dir);
                Debug.Log("[PrincessControllerStage2] 버퍼 입력 사용, 다음 이동 시작: " + dir);
            }
        }
    }

    void StartMove(Vector2 dir)
    {
        if (dir == Vector2.zero) return;

        // 1) 목적지 계산
        Vector3 dest = transform.position + (Vector3)(dir * stepSize);

        // 2) 목적지에 벽이 있는지 미리 확인 (이동 시작 전에 체크)
        bool blocked = Physics2D.OverlapCircle(dest, collisionRadius, obstacleLayer) != null;
        if (blocked)
        {
            Debug.Log("[PrincessControllerStage2] 벽에 막혀서 이동 취소: " + dest);
            return; // 아예 이동 시작을 안 함 → isMoving 그대로 false
        }

        // 3) 이동 시작
        targetPos = dest;
        isMoving = true;

        // 좌우 방향에 따라 스프라이트 뒤집기
        if (spriteRenderer != null)
        {
            // 기본이 왼쪽 보게 그려졌다고 가정
            if (dir.x > 0.01f)
                spriteRenderer.flipX = true;   // 오른쪽 이동
            else if (dir.x < -0.01f)
                spriteRenderer.flipX = false;  // 왼쪽 이동
        }

        Debug.Log("[PrincessControllerStage2] 새 목표 위치: " + targetPos);
    }

}
