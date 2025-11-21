using UnityEngine;
using TMPro;
using UnityEngine.UI; // 🌟 추가: UI 이미지 컴포넌트를 사용하기 위해 필요합니다.

public class HUDManagerStage1 : MonoBehaviour
{
    [Header("UI Text 요소 연결")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;

    // 🌟 수정: 체력 텍스트 대신 이미지 배열로 대체
    [Header("HP 이미지 설정 (4개)")]
    [Tooltip("순서대로 4개의 HP 이미지 오브젝트를 연결하세요.")]
    public Image[] healthIcons = new Image[4]; // 4개의 HP 아이콘
    [Tooltip("꽉 찬 HP 이미지 Sprite를 연결하세요.")]
    public Sprite fullHealthSprite;
    [Tooltip("잃은 HP 이미지 Sprite를 연결하세요.")]
    public Sprite emptyHealthSprite;

    private float gameTime = 0f;
    private bool isGameActive = true; 

    void Start()
    {
        // 텍스트 컴포넌트와 이미지 배열 연결 확인
        if (timeText == null || scoreText == null || healthIcons.Length != 4)
        {
            Debug.LogError("HUDManager: UI 컴포넌트 연결 또는 HP 이미지 4개가 연결되지 않았습니다!");
            enabled = false;
        }
        
        if (fullHealthSprite == null || emptyHealthSprite == null)
        {
            Debug.LogError("HUDManager: fullHealthSprite 또는 emptyHealthSprite가 연결되지 않았습니다!");
        }
    }

    void Update()
    {
        if (isGameActive && Time.timeScale > 0) 
        {
            gameTime += Time.deltaTime;
            UpdateTime(gameTime);
        }
    }

    // ---------------------------
    // 외부에서 호출되는 공용 UI 업데이트 메서드
    // ---------------------------

    /// <summary> 🌟 수정: 플레이어 체력 이미지 업데이트 </summary>
    public void UpdateHealth(int currentHealth)
    {
        // 4개의 HP 아이콘을 순회하며 체력 상태에 맞게 Sprite를 교체합니다.
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                // i번째 아이콘이 현재 체력보다 작다면 (즉, 아직 잃지 않았다면)
                if (i < currentHealth)
                {
                    healthIcons[i].sprite = fullHealthSprite;
                }
                else
                {
                    // i번째 아이콘이 현재 체력보다 크거나 같다면 (즉, 잃었다면)
                    healthIcons[i].sprite = emptyHealthSprite;
                }
            }
        }
        
        // 기존의 healthText.text 업데이트 로직은 필요 없으므로 제거했습니다.
    }

    /// <summary> 게임 점수 업데이트 </summary>
    public void UpdateScore(int newScore)
    {
        scoreText.text = $"Eggs: {newScore}";
    }

    /// <summary> 게임 진행 시간 표시 포맷 (분:초) </summary>
    private void UpdateTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60f);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60f);

        timeText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    /// <summary> 시간 업데이트 활성/비활성화 </summary>
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        if (active)
        {
            gameTime = 0f; 
        }
    }
}