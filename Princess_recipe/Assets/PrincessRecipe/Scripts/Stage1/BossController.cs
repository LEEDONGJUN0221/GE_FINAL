using UnityEngine;

public class BossController : MonoBehaviour
{
    // 🌟 추가: 보스의 SpriteRenderer 컴포넌트와 격퇴 시 스프라이트
    private SpriteRenderer spriteRenderer;

    [Header("스프라이트 설정")]
    // 💡 유니티 에디터에서 보스가 격퇴되었을 때 보여줄 스프라이트를 할당하세요.
    public Sprite defeatedBossSprite; 

    [Header("보스 달걀 요구량")]
    public int requiredEggs = 5;
    private int receivedEggs = 0; 
    private bool isDefeated = false; 

    // GameManagerStage1 참조 (클리어 처리를 위해 필요)
    private GameManagerStage1 gameManager; 

    [Header("몬스터 난이도 조절")]
    // 🌟 추가: 달걀 1개당 증가시킬 몬스터 이동 속도 값
    public float monsterSpeedIncreasePerEgg = 1.0f;

    void Awake()
    {
        // SpriteRenderer 컴포넌트 참조 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("BossController: SpriteRenderer 컴포넌트를 찾을 수 없습니다.", this);
        }
    }

    void Start()
    {
        // GameManagerStage1 인스턴스 찾기
        //FindObjectOfType 2024ver 이후로 Deprecated 되어 warning 떠서 수정했습니다.
        gameManager = FindAnyObjectByType<GameManagerStage1>();
        if (gameManager == null)
        {
            Debug.LogError("씬에서 GameManagerStage1를 찾을 수 없습니다! 보스 기능이 정상 작동하지 않습니다.");
        }
    }

    /// <summary>
    /// 플레이어로부터 달걀을 받는 메서드.
    /// </summary>
    public bool ReceiveEgg()
    {
        if (isDefeated)
        {
            Debug.Log("보스는 이미 격퇴되었습니다.");
            return false;
        }

        receivedEggs++;
        
        // 점수 추가 (GameManagerStage1 연결 필요)
        if (gameManager != null)
        {
            gameManager.AddScore(1); 
            IncreaseMonsterSpeed();

            Debug.Log($"보스가 달걀을 받았습니다. 현재 {receivedEggs} / {requiredEggs}");
        }

        Debug.Log($"보스가 달걀을 받았습니다. 현재 {receivedEggs} / {requiredEggs}");

        if (receivedEggs >= requiredEggs)
        {
            DefeatBoss();
        }

        return true;
    }
    void IncreaseMonsterSpeed()
    {
        // 씬에서 모든 MonsterPatrol 컴포넌트를 찾습니다.
        // 2024ver 이후로 FindObjectsOfType<T>()는 FindObjectsByType<T>()로 사용하시는 것을 권장합니다.
        MonsterPatrol[] monsters = FindObjectsByType<MonsterPatrol>(FindObjectsSortMode.None);

        foreach (MonsterPatrol monster in monsters)
        {
            // 각 몬스터의 속도를 증가시킵니다.
            monster.IncreaseSpeed(monsterSpeedIncreasePerEgg);
        }
        
        if (monsters.Length > 0)
        {
            Debug.Log($"달걀 수신으로 인해 총 {monsters.Length} 마리 몬스터의 이동 속도가 {monsterSpeedIncreasePerEgg}만큼 증가했습니다.");
        }
    }
   
    void StopAllMonsters()
    {
        // 씬에서 모든 MonsterPatrol 컴포넌트를 찾습니다.
        MonsterPatrol[] monsters = FindObjectsByType<MonsterPatrol>(FindObjectsSortMode.None);

        foreach (MonsterPatrol monster in monsters)
        {
            // 각 몬스터의 멈춤 메서드를 호출합니다.
            monster.StopMonster();
        }
        
        if (monsters.Length > 0)
        {
            Debug.Log($"총 {monsters.Length} 마리 몬스터의 움직임이 멈췄습니다.");
        }
    }

    /// <summary>
    /// 보스가 격퇴되었을 때의 처리. (스프라이트 변경)
    /// </summary>
    void DefeatBoss()
    {
        isDefeated = true;
        Debug.Log("🎉 보스 격퇴! 스테이지 클리어!");
        StopAllMonsters();
        
        // 🌟 1. 보스 스프라이트를 변경합니다.
        if (spriteRenderer != null && defeatedBossSprite != null)
        {
            spriteRenderer.sprite = defeatedBossSprite;
            
            // 🌟 2. 필요하다면, 보스 오브젝트의 콜라이더를 비활성화합니다.
            Collider2D bossCollider = GetComponent<Collider2D>();
            if (bossCollider != null)
            {
                bossCollider.enabled = false;
            }
        }
        else
        {
            // 스프라이트 변경 실패 시: 기존 로직대로 오브젝트 비활성화
            Debug.LogWarning("defeatedBossSprite가 할당되지 않아 오브젝트를 비활성화합니다.", this);
            gameObject.SetActive(false); 
        }
        
        // 3. GameManagerStage1에 게임 클리어 처리를 요청합니다.
        if (gameManager != null)
        {
            gameManager.GameClear(); 
        }
        
        // gameObject.SetActive(false); // 👈 이 코드는 스프라이트 변경으로 대체되었습니다.
    }
}