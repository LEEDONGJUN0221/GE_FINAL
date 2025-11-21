using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerStage1 : MonoBehaviour
{
    private HUDManagerStage1 hudManager;
    
    [Header("UI 패널 연결")]
    public GameObject gameClearPanel; 
    public GameObject gameOverPanel; 
    
    [Header("게임 흐름 설정")]
    public string nextSceneName = "Stage2"; 
    
    // 현재 점수 값은 GameManagerStage1이 직접 관리
    private int currentScore = 0; 
    
    // 🌟 추가: 체력 변수 관리
    private int maxHealth = 4; // 최대 체력 (HP 아이콘 수)
    private int currentHealth;

    void Awake()
    {
        // ... 기존 코드 ...
        hudManager = FindObjectOfType<HUDManagerStage1>(); 
        if (hudManager == null)
        {
            Debug.LogError("HUDManagerStage1을 씬에서 찾을 수 없습니다! HUDManager 스크립트를 HUD 오브젝트에 부착하고 확인해주세요.");
        }
    }

    void Start()
    {
        // 초기 UI 상태 및 게임 시간 설정
        gameClearPanel.SetActive(false); 
        gameOverPanel.SetActive(false); 
        Time.timeScale = 1f; 
        
        // 🌟 수정: 초기 점수 및 체력 설정
        currentScore = 0;
        currentHealth = maxHealth; // 시작 시 최대 체력으로 설정
        
        if (hudManager != null)
        {
            hudManager.UpdateScore(currentScore);
            hudManager.UpdateHealth(currentHealth); // 🌟 HUD에 초기 체력 전달
            hudManager.SetGameActive(true); 
        }
    }
    
    // ... AddScore 메서드 (변동 없음) ...
    public void AddScore(int amount)
    {
        currentScore += amount;
        
        if (hudManager != null)
        {
            hudManager.UpdateScore(currentScore);
        }
    }

    // ---------------------------
    // 💀 체력 감소 메서드
    // ---------------------------
    
    /// <summary>
    /// 플레이어의 체력을 감소시키고 게임 오버를 확인합니다.
    /// </summary>
    public void TakeDamage()
    {
        currentHealth--;
        
        if (hudManager != null)
        {
            hudManager.UpdateHealth(currentHealth); // HUD 업데이트
        }
        
        if (currentHealth <= 0)
        {
            // 🌟 체력이 0 이하면 게임 오버 처리
            GameOver();
        }
    }

    // ---------------------------
    // 🏆 성공/실패 처리 메서드
    // ---------------------------

    public void GameClear()
    {
        gameClearPanel.SetActive(true); 
        Time.timeScale = 0f;
        if (hudManager != null) hudManager.SetGameActive(false);
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

    public void GameOver()
    {
        gameOverPanel.SetActive(true); 
        Time.timeScale = 0f; 
        if (hudManager != null) hudManager.SetGameActive(false); 
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; 
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}