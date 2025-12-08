using UnityEngine;
using TMPro;

public class GameManagerStage2 : MonoBehaviour
{
    public static GameManagerStage2 Instance;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;

    [Header("Player HP")]
    public int maxHP = 4;
    [HideInInspector]
    public int currentHP;          // 인스펙터에서 안 보이게, 코드에서만 관리

    [Header("Chocolate Score")]
    public int chocolateGoal = 30;
    public int chocolateCount = 0;
    public TextMeshProUGUI chocolateText;

    private WarningManagerStage2 warningManager;

    // ------------ 생명주기 ------------

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 🔥 인스펙터 값과 상관없이 무조건 maxHP로 시작
        currentHP = maxHP;
        Debug.Log($"[GameManagerStage2] Awake - HP 초기화: {currentHP}/{maxHP}");
    }

    void Start()
    {
        UpdateChocolateUI();
        // 필요하면 바로 시작
        // StartStage();
    }

    // ------------ 스테이지 시작 ------------

    public void StartStage()
    {
        Debug.Log("Stage2 시작!");

        if (startPanel != null)
            startPanel.SetActive(false);

        if (warningManager == null)
            warningManager = FindAnyObjectByType<WarningManagerStage2>();

        if (warningManager != null)
            warningManager.enabled = true;
        else
            Debug.LogError("WarningManagerStage2를 찾지 못했습니다.");
    }

    // ------------ 데미지 / HP ------------

    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return; // 이미 죽었으면 무시

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[GameManagerStage2] HP 감소: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            OnPlayerDeath();
        }
    }

    void OnPlayerDeath()
    {
        Debug.Log("[GameManagerStage2] 플레이어 사망");

        if (warningManager != null)
            warningManager.enabled = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // ------------ 초콜릿 ------------

    public void AddChocolate(int amount)
    {
        chocolateCount += amount;
        if (chocolateCount > chocolateGoal)
            chocolateCount = chocolateGoal;

        UpdateChocolateUI();
        Debug.Log($"[GameManagerStage2] 초콜릿: {chocolateCount}/{chocolateGoal}");

        if (chocolateCount >= chocolateGoal)
            OnStageClear();
    }

    void UpdateChocolateUI()
    {
        if (chocolateText != null)
            chocolateText.text = $"{chocolateCount} / {chocolateGoal}";
    }

    void OnStageClear()
    {
        Debug.Log("[GameManagerStage2] Stage2 클리어!");

        if (warningManager != null)
            warningManager.enabled = false;

        if (gameClearPanel != null)
            gameClearPanel.SetActive(true);
    }
}
