using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("이동 설정 (참조용, 실제 이동 로직 없음)")]
    // private Rigidbody2D rb; // 이동 로직이 없어 제외

    [Header("리스폰 설정")] // 리스폰 위치 설정
    public Vector3 respawnPosition = new Vector3(0, 0, 0); // 시작 지점 좌표 (유니티 인스펙터에서 설정)
    private Rigidbody2D rb; // Respawn()에서 속도 초기화를 위해 유지

    [Header("달걀 설정")]
    public int currentEggs = 0;    // 현재 소유 달걀 수
    public int maxEggs = 1;        // 최대 보유 달걀 수
    public int minEggs = 0;        // 최소 보유 달걀 수
    
    // 보스 상호작용 관련 변수
    private BossController nearbyBoss = null; // 근처에 있는 보스 컨트롤러 참조

    void Start()
    {
        // Rigidbody2D 컴포넌트를 가져옵니다 (Respawn에서 사용).
        rb = GetComponent<Rigidbody2D>(); 
        // 시작 시 현재 위치를 리스폰 위치로 설정합니다.
        respawnPosition = transform.position; 
    }

    void Update()
    {
        // 보스에게 달걀 전달 입력
        // (원래 PlayerMove의 Update()에 있던 로직 중 이동과 무관한 부분만 유지)
        if (Input.GetKeyDown(KeyCode.Space) && nearbyBoss != null)
        {
            GiveEggToBoss();
        }
    }

    // void FixedUpdate() { /* 이동 로직 제외 */ }

    /// <summary>
    /// 현재 소유한 달걀을 근처 보스에게 전달합니다.
    /// </summary>
    void GiveEggToBoss()
    {
        if (currentEggs > 0 && nearbyBoss != null)
        {
            // 달걀을 보스에게 전달 시도
            if (nearbyBoss.ReceiveEgg())
            {
                currentEggs--;
                Debug.Log("보스에게 달걀 전달 성공! 현재: " + currentEggs);
            }
        }
        else if (currentEggs <= 0)
        {
            Debug.Log("전달할 달걀이 없습니다.");
        }
    }

    /// <summary>
    /// 플레이어를 리스폰 위치로 이동시키고 상태를 초기화합니다.
    /// </summary>
    void Respawn()
    {
        // 1. 플레이어 위치를 리스폰 지점으로 이동
        transform.position = respawnPosition;
        
        // 2. 달걀 수 초기화
        currentEggs = minEggs; // 최소값 0으로 초기화
        
        // 3. Rigidbody 속도 초기화 (충돌 후 관성 제거)
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        Debug.Log("몬스터와 충돌하여 시작 지점(" + respawnPosition + ")으로 리스폰되었습니다. 달걀: " + currentEggs);
    }
        
    // 충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 일반 달걀 획득 로직
        if (collision.CompareTag("Stage1_Egg"))
        {
            if (currentEggs < maxEggs)
            {
                currentEggs += 1;
                Debug.Log("달걀 획득! 현재: " + currentEggs);
            }
            else
            {
                Debug.Log("달걀 최대 보유량 도달!");
            }
        }

        // 2. 몬스터 충돌 로직
        if (collision.CompareTag("Stage1_Monster"))
        {
            if (currentEggs == maxEggs)
            {
                currentEggs -= 1;
                Debug.Log("달걀 감소! 현재: " + currentEggs);
                Respawn();
            }
            else
            {
                Debug.Log("달걀이 없어요!!");
                Respawn();
            }
        }

        // 3. 보스 진입 시 nearbyBoss 설정
        if (collision.CompareTag("Stage1_Boss"))
        {
            nearbyBoss = collision.GetComponent<BossController>();
            if (nearbyBoss != null)
            {
                Debug.Log("보스 근처에 진입했습니다. Space 키로 달걀을 전달할 수 있습니다.");
            }
        }
    }

    // 보스 구역 이탈 시 nearbyBoss 해제
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Stage1_Boss"))
        {
            if (nearbyBoss != null)
            {
                nearbyBoss = null;
                Debug.Log("보스 구역에서 벗어났습니다.");
            }
        }
    }
}