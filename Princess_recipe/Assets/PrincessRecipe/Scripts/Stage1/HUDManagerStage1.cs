using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManagerStage1 : MonoBehaviour
{
    [Header("UI Text 요소 연결")]
    public TextMeshProUGUI timeText;     // ⬅ 시간 텍스트만 사용

    [Header("HP 이미지 설정 (4개)")]
    public Image[] healthIcons = new Image[4];
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;

    [Header("SCORE(EGG) 이미지 설정 (6개)")]
    public Image[] scoreIcons = new Image[6];
    public Sprite EmptyScoreSprite;
    public Sprite FullScoreSprite;

    private float gameTime = 0f;
    private bool isGameActive = true;

    void Start()
    {
        // 시간 텍스트와 HP 아이콘 개수만 체크
        if (timeText == null || healthIcons == null || healthIcons.Length != 4)
        {
            Debug.LogError("HUDManager: timeText 또는 HP 이미지 4개가 제대로 연결되지 않았습니다!");
            enabled = false;
            return;
        }

        if (fullHealthSprite == null || emptyHealthSprite == null)
        {
            Debug.LogError("HUDManager: fullHealthSprite 또는 emptyHealthSprite가 연결되지 않았습니다!");
        }

        if (EmptyScoreSprite == null || FullScoreSprite == null)
        {
            Debug.LogError("HUDManager: EmptyScoreSprite 또는 FullScoreSprite가 연결되지 않았습니다!");
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

    // ------------ 외부에서 호출 ---------------

    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] == null) continue;

            if (i < currentHealth)
                healthIcons[i].sprite = fullHealthSprite;
            else
                healthIcons[i].sprite = emptyHealthSprite;
        }
    }

    public void UpdateScore(int newScore)
    {
        for (int i = 0; i < scoreIcons.Length; i++)
        {
            if (scoreIcons[i] == null) continue;

            if (i < newScore)
                scoreIcons[i].sprite = FullScoreSprite;
            else
                scoreIcons[i].sprite = EmptyScoreSprite;
        }
    }

    // ------------ 내부용 ---------------

    private void UpdateTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60f);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60f);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void SetGameActive(bool active)
    {
        isGameActive = active;
        // 필요하면 여기서만 게임 재시작할 때 초기화
        // gameTime = 0f;
    }
}
