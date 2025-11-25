using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // ... (기존 변수 유지) ...
    [Header("리스폰 설정")]
    public Vector3 respawnPosition = new Vector3(0, 0, 0); 
    private Rigidbody2D rb; 

    [Header("달걀 설정")]
    public int currentEggs = 0;    
    public int maxEggs = 1;        
    public int minEggs = 0;        
    
    // 보스 상호작용 관련 변수
    private BossController nearbyBoss = null; 

    // 🌟 추가: GameManager 참조
    private GameManagerStage1 gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        respawnPosition = transform.position; 
        
        // 🌟 추가: GameManager 참조 가져오기
        //FindObjectOfType 2024ver 이후로 Deprecated 되어 warning 떠서 수정했습니다.
        gameManager = FindAnyObjectByType<GameManagerStage1>();
        if (gameManager == null)
        {
            Debug.LogError("GameManagerStage1을 씬에서 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        // 🌟 수정: GiveEggToBoss 로직 (오류 발생 부분)
        if (Input.GetKeyDown(KeyCode.Space) && nearbyBoss != null)
        {
            GiveEggToBoss();
        }
    }

    /// <summary>
    /// 현재 소유한 달걀을 근처 보스에게 전달합니다. (누락된 메서드 추가)
    /// </summary>
    void GiveEggToBoss()
    {
        if (currentEggs > 0 && nearbyBoss != null)
        {
            // 달걀을 보스에게 전달 시도
            // BossController 스크립트에 ReceiveEgg() 메서드가 있어야 작동합니다.
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
        
        // 2. 달걀 수 초기화 (충돌 로직에서 이미 처리되므로 여기서는 위치만)
        
        // 3. Rigidbody 속도 초기화 (충돌 후 관성 제거)
        if (rb != null)
        {
            // RigidbodyType2D.Kinematic을 사용하면 linearVelocity 대신 velocity를 사용하는 것이 일반적입니다.
            rb.linearVelocity = Vector2.zero; 
        }
        
        Debug.Log("몬스터와 충돌하여 시작 지점(" + respawnPosition + ")으로 리스폰되었습니다.");
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
                
                if (gameManager != null)
                {
                    // 획득 시 점수(Eggs) 업데이트
                    gameManager.AddScore(1); 
                }
            }
            else
            {
                Debug.Log("달걀 최대 보유량 도달!");
            }
            // 획득 후 달걀 오브젝트 파괴 로직이 필요할 수 있습니다. 
            // Destroy(collision.gameObject);
        }

        // 2. 몬스터 충돌 로직 (HP 감소 및 리스폰)
        if (collision.CompareTag("Stage1_Monster"))
        {
            if (gameManager != null)
            {
                gameManager.TakeDamage(); // HP 1 감소 및 HUD 업데이트
            }

            if (currentEggs == maxEggs)
            {
                // 달걀이 있을 경우 달걀을 잃음
                currentEggs = minEggs; 
                Respawn();
            }
            else
            {
                // 달걀이 없을 경우
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