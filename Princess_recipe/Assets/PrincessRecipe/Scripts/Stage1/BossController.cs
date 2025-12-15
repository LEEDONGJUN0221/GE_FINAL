using UnityEngine;

public class BossController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Idle State 이름(Animator 상태 이름 그대로)")]
    public string idleStateA = "BossIdle";   // A용 Idle 상태 이름
    public string idleStateB = "BossIdleB";  // B용 Idle 상태 이름

    [Header("선택 인덱스")]
    public int referenceChoiceIndex = 1;

    [Header("Defeat Trigger")]
    public string defeatTriggerName = "Defeat";

    public int requiredEggs = 3;
    private int receivedEggs = 0;
    private bool isDefeated = false;

    private GameManagerStage1 gameManager;

    public float monsterSpeedIncreasePerEgg = 0.5f;

    private int cachedChoice = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // choice 읽기
        cachedChoice = GetChoice(referenceChoiceIndex);
        if (cachedChoice == -1) cachedChoice = 0;

        // ⭐ default 무시하고, 시작 상태를 강제로 “즉시” 바꿈
        if (animator != null)
        {
            animator.Update(0f); // 한 번 갱신해서 준비
            animator.Play(cachedChoice == 0 ? idleStateA : idleStateB, 0, 0f);
            animator.Update(0f);
        }
    }

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManagerStage1>();
    }

    private int GetChoice(int idx)
    {
        if (RunData.I == null) return -1;
        if (idx == 1) return RunData.I.choice0;
        if (idx == 2) return RunData.I.choice1;
        if (idx == 3) return RunData.I.choice2;
        return -1;
    }

    public bool ReceiveEgg()
    {
        if (isDefeated) return false;

        receivedEggs++;

        if (gameManager != null)
        {
            gameManager.AddScore(1);
            IncreaseMonsterSpeed();
        }

        if (receivedEggs >= requiredEggs)
            DefeatBoss();

        return true;
    }

    void IncreaseMonsterSpeed()
    {
        MonsterPatrol[] monsters = FindObjectsByType<MonsterPatrol>(FindObjectsSortMode.None);
        foreach (var m in monsters)
            m.IncreaseSpeed(monsterSpeedIncreasePerEgg);
    }

    void StopAllMonsters()
    {
        MonsterPatrol[] monsters =
            FindObjectsByType<MonsterPatrol>(FindObjectsSortMode.None);

        foreach (MonsterPatrol monster in monsters)
        {
            monster.Die();   // ★ 핵심
        }
    }


    void DefeatBoss()
    {
        if (isDefeated) return;
        isDefeated = true;

        StopAllMonsters();

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (animator != null)
            animator.SetTrigger(defeatTriggerName);

        if (gameManager != null)
            gameManager.GameClear();
    }
}
