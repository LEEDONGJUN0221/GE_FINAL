using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerStage2 : MonoBehaviour
{
    public static GameManagerStage2 Instance;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gameOverPanel;

    [Header("★ Clear는 MapUI가 처리")]
    public MapUI mapUI;          // Stage2 씬에 있는 MapUI
    public string nextSceneName = "Stage3";
    public int choiceIndex = 2;  // Stage2 → Stage3 선택

    [Header("Player HP")]
    public int maxHP = 4;
    public int currentHP;

    public Image[] hpIcons;
    public Sprite hpFullSprite;
    public Sprite hpBrokenSprite;

    [Header("Chocolate")]
    public int chocolateGoal = 30;
    public int chocolateCount = 0;
    public TextMeshProUGUI chocolateText;

    private WarningManagerStage2 warningManager;

    // =====================
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        currentHP = maxHP;

        if (mapUI == null)
            mapUI = FindAnyObjectByType<MapUI>();
    }

    void Start()
    {
        UpdateHPUI();
        UpdateChocolateUI();
    }

    // =====================
    public void StartStage()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (warningManager == null)
            warningManager = FindAnyObjectByType<WarningManagerStage2>();

        if (warningManager != null)
            warningManager.enabled = true;
    }

    // =====================
    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        UpdateHPUI();

        if (currentHP <= 0)
            OnPlayerDeath();
    }

    void UpdateHPUI()
    {
        for (int i = 0; i < hpIcons.Length; i++)
        {
            if (hpIcons[i] == null) continue;
            hpIcons[i].sprite = (i < currentHP) ? hpFullSprite : hpBrokenSprite;
        }
    }

    void OnPlayerDeath()
    {
        if (warningManager != null)
            warningManager.enabled = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // =====================
    public void AddChocolate(int amount)
    {
        chocolateCount = Mathf.Min(chocolateGoal, chocolateCount + amount);
        UpdateChocolateUI();

        if (chocolateCount >= chocolateGoal)
            OnStageClear();
    }

    void UpdateChocolateUI()
    {
        if (chocolateText != null)
            chocolateText.text = $"{chocolateCount} / {chocolateGoal}";
    }

    // =====================
    void OnStageClear()
    {
        Debug.Log("[Stage2] CLEAR");

        if (warningManager != null)
            warningManager.enabled = false;

        // ⭐ 핵심: 여기서 씬 이동 안 함
        if (mapUI != null)
        {
            mapUI.OpenChoice(choiceIndex, nextSceneName);
        }
        else
        {
            Debug.LogError("MapUI 없음!");
            Time.timeScale = 0f;
        }
    }
}
