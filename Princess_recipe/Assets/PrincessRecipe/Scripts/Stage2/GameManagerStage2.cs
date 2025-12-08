using UnityEngine;
using UnityEngine.UI;
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
    public int currentHP;

    [Tooltip("왼쪽 위 딸기 HP 아이콘들 (위에서 아래 순서대로)")]
    public Image[] hpIcons;         // HP_1, HP_2, HP_3, HP_4
    public Sprite hpFullSprite;     // 멀쩡한 딸기
    public Sprite hpBrokenSprite;   // 깨진 딸기

    [Header("Chocolate Score")]
    public int chocolateGoal = 30;
    public int chocolateCount = 0;
    public TextMeshProUGUI chocolateText;

    private WarningManagerStage2 warningManager;

    // ============== 생명 주기 ==============
    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // HP는 무조건 maxHP로 시작
        currentHP = maxHP;
        Debug.Log($"[GameManagerStage2] Awake - HP 초기화: {currentHP}/{maxHP}");
    }

    void Start()
    {
        UpdateHPUI();
        UpdateChocolateUI();
        // StartStage();  // 시작 패널 없이 바로 시작하고 싶으면 주석 해제
    }

    // ============== 스테이지 시작 ==============
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

    // ============== HP / 데미지 ==============
    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;  // 이미 죽었으면 무시

        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[GameManagerStage2] HP 감소: {currentHP}/{maxHP}");

        UpdateHPUI();  // 🔥 데미지 들어올 때마다 아이콘 즉시 갱신

        if (currentHP <= 0)
        {
            OnPlayerDeath();
        }
    }

    void UpdateHPUI()
    {
        if (hpIcons == null || hpIcons.Length == 0)
        {
            Debug.LogWarning("[GameManagerStage2] hpIcons가 비어있음");
            return;
        }

        int hp = Mathf.Clamp(currentHP, 0, hpIcons.Length);

        for (int i = 0; i < hpIcons.Length; i++)
        {
            if (hpIcons[i] == null) continue;

            hpIcons[i].sprite = (i < hp) ? hpFullSprite : hpBrokenSprite;
        }

        Debug.Log($"[GameManagerStage2] HP UI 갱신 - HP: {hp}");
    }

    void OnPlayerDeath()
    {
        Debug.Log("[GameManagerStage2] 플레이어 사망");

        if (warningManager != null)
            warningManager.enabled = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // ============== 초콜릿 ==============
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
