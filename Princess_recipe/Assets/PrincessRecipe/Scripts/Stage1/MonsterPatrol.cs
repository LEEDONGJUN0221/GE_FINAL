using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    [Header("순찰 지점 설정")]
    // 몬스터가 순찰할 첫 번째 지점 (월드 좌표)
    public Vector2 point1; 
    // 몬스터가 순찰할 두 번째 지점 (월드 좌표)
    public Vector2 point2; 
    
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    

    private Vector2 currentTarget;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 시작 시 현재 위치에서 가장 가까운 지점을 목표로 설정
        if (Vector2.Distance(transform.position, point1) < Vector2.Distance(transform.position, point2))
        {
            currentTarget = point2;
        }
        else
        {
            currentTarget = point1;
        }
        
        // Rigidbody2D가 있다면 Kinematic으로 설정하는 것을 권장합니다.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        // 1. 목표 지점으로 이동
        transform.position = Vector2.MoveTowards(
            transform.position, 
            currentTarget, 
            moveSpeed * Time.deltaTime
        );

        // 2. 목표 지점에 도달했는지 확인하고 방향 전환
        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            // 목표를 반대편 지점으로 변경
            if (currentTarget == point1)
            {
                currentTarget = point2;
            }
            else
            {
                currentTarget = point1;
            }
        }
        
        // 3. 스프라이트 뒤집기 (시각적 방향 전환)
        UpdateSpriteDirection();
    }
    
    void UpdateSpriteDirection()
    {
        if (spriteRenderer != null)
        {
            // 현재 이동 방향을 계산
            Vector2 direction = currentTarget - (Vector2)transform.position;
            
            // X축 방향에 따라 스프라이트 뒤집기
            if (direction.x > 0.01f) // 오른쪽으로 이동 중
            {
                spriteRenderer.flipX = false;
            }
            else if (direction.x < -0.01f) // 왼쪽으로 이동 중
            {
                spriteRenderer.flipX = true;
            }
        }
    }
    public void IncreaseSpeed(float speedIncreaseAmount)
    {
        moveSpeed += speedIncreaseAmount;
        Debug.Log($"{gameObject.name}의 속도 증가! 현재 속도: {moveSpeed}");
    }
   
    public void StopMonster()
    {
        // moveSpeed를 0으로 설정하여 Update에서의 MoveTowards를 멈춥니다.
        moveSpeed = 0f;
        Debug.Log($"{gameObject.name} 몬스터가 멈췄습니다.");
    }
}
