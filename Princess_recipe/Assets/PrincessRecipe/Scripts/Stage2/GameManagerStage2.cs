using UnityEngine;

public class GameManagerStage2 : MonoBehaviour
{
    public static GameManagerStage2 Instance;

    [Header("UI Panels")]
    public GameObject startPanel;


    [Header("Player HP")]
    public int maxHP = 4;
    public int currentHP = 4;

    private WarningManagerStage2 warningManager;

    public void StartStage()
    {
        Debug.Log("Stage2 시작!");

        // 🔥 1) Start 버튼 UI 패널 숨기기
        if (startPanel != null)
            startPanel.SetActive(false);

        // 🔥 2) WarningManager 켜기
        if (warningManager == null)
            warningManager = FindAnyObjectByType<WarningManagerStage2>();

        if (warningManager != null)
            warningManager.enabled = true;
        else
            Debug.LogError("WarningManagerStage2를 찾지 못했습니다.");
    }



    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // HP 감소
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;

        Debug.Log("Stage2 HP: " + currentHP);

        // TODO: 여기서 UI 업데이트 / 죽음 처리 연결할 수 있음
    }
}
