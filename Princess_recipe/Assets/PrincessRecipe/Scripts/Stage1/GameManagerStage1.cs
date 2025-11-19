using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerStage1 : MonoBehaviour // 👈 클래스 이름 변경
{
    // HUDManager 참조를 저장
    private HUDManagerStage1 hudManager;
    
    [Header("UI 패널 연결")]
    public GameObject gameClearPanel; 
    public GameObject gameOverPanel; 
    
    [Header("게임 흐름 설정")]
    public string nextSceneName = "Stage2"; 
    
    // 현재 점수 값은 GameManagerStage1이 직접 관리
    private int currentScore = 0; 

    void Awake()
        {
            // 씬에서 HUDManager를 찾아 참조를 얻습니다.
            // 🚨 만약 HUD 스크립트가 'HUDManager.cs'라면 아래와 같이 수정해야 합니다. 
            // hudManager = FindObjectOfType<HUDManager>(); 

            // 현재 provided code: 
            hudManager = FindObjectOfType<HUDManagerStage1>(); 
            if (hudManager == null)
            {
                // 이 로그는 HUD 스크립트 이름이 'HUDManagerStage1'인데도 못 찾을 때 발생합니다.
                Debug.LogError("HUDManagerStage1을 씬에서 찾을 수 없습니다! HUDManager 스크립트를 HUD 오브젝트에 부착하고 확인해주세요.");
            }
        }

    void Start()
    {
        // 초기 UI 상태 및 게임 시간 설정
        gameClearPanel.SetActive(false); 
        gameOverPanel.SetActive(false); 
        Time.timeScale = 1f; 
        
        // 초기 점수와 HUD 업데이트
        currentScore = 0;
        if (hudManager != null)
        {
            hudManager.UpdateScore(currentScore);
            hudManager.SetGameActive(true); // 시간 업데이트 시작
        }
    }
    
    // ---------------------------
    // 점수 관리 메서드 (BossController 등에서 호출)
    // ---------------------------

    /// <summary>
    /// 점수를 증가시키고 HUD를 업데이트합니다.
    /// </summary>
    public void AddScore(int amount)
    {
        currentScore += amount;
        
        if (hudManager != null)
        {
            hudManager.UpdateScore(currentScore);
        }
    }

    // ---------------------------
    // 🏆 성공 처리 메서드
    // ---------------------------

    public void GameClear()
    {
        gameClearPanel.SetActive(true); 
        Time.timeScale = 0f;
        if (hudManager != null) hudManager.SetGameActive(false); // 시간 업데이트 중지
    }

    public void LoadNextStage()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(nextSceneName);
    }
    
    public void QuitGame()
    {
        Time.timeScale = 1f; 
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ---------------------------
    // 💀 실패 처리 메서드
    // ---------------------------

    public void GameOver()
    {
        gameOverPanel.SetActive(true); 
        Time.timeScale = 0f; 
        if (hudManager != null) hudManager.SetGameActive(false); // 시간 업데이트 중지
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; 
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}