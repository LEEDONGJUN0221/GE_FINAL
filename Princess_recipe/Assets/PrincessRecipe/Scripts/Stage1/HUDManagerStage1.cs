using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필수입니다!

public class HUDManagerStage1 : MonoBehaviour
{
    [Header("UI Text 요소 연결")]
    // 인스펙터에서 Canvas의 TextMeshPro 오브젝트들을 연결하세요.
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText; // 이제 HUDManager가 점수 표시만 담당합니다.

    private float gameTime = 0f;
    private bool isGameActive = true; 

    void Start()
    {
        // 모든 텍스트 컴포넌트 연결 확인
        if (healthText == null || timeText == null || scoreText == null)
        {
            Debug.LogError("HUDManager: 모든 TextMeshProUGUI 컴포넌트가 연결되지 않았습니다!");
            enabled = false;
        }
    }

    void Update()
    {
        // 게임이 활성화 상태이고 시간이 멈추지 않은 경우에만 시간 업데이트
        if (isGameActive && Time.timeScale > 0) 
        {
            gameTime += Time.deltaTime;
            UpdateTime(gameTime);
        }
    }

    // ---------------------------
    // 외부에서 호출되는 공용 UI 업데이트 메서드
    // ---------------------------

    /// <summary> 플레이어 체력 업데이트 </summary>
    public void UpdateHealth(int newHealth)
    {
        healthText.text = $"HP: {newHealth}";
    }

    /// <summary> 게임 점수 업데이트 (GameManager에서 호출됨) </summary>
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
        // 게임이 재시작되면 시간도 초기화 (선택 사항)
        if (active)
        {
            gameTime = 0f; 
        }
    }
}