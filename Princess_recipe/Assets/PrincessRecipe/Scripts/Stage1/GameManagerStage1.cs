using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 🌟 코루틴을 사용하기 위해 추가

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

    // GameClear 여러 번 호출 방지
    private bool isGameClear = false;

    // 🎵 BGM 설정
    [Header("BGM 설정")]
    [Tooltip("스테이지 진행 중 재생할 BGM")]
    public AudioClip stageBGM;
    [Tooltip("게임 클리어 시 재생할 BGM")]
    public AudioClip gameClearBGM;
    [Tooltip("게임 오버 시 재생할 BGM")]
    public AudioClip gameOverBGM;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    private AudioSource bgmSource;   // BGM 전용 AudioSource


    void Awake()
    {
        hudManager = FindAnyObjectByType<HUDManagerStage1>(); 
        if (hudManager == null)
        {
            Debug.LogError("HUDManagerStage1을 씬에서 찾을 수 없습니다! HUDManager 스크립트를 HUD 오브젝트에 부착하고 확인해주세요.");
        }
    }

    void Start()
    {
        // 🔊 BGM AudioSource 세팅
        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

        // 스테이지 기본 BGM 재생
        if (stageBGM != null)
        {
            PlayBGM(stageBGM, true);
        }

        // 초기 UI 상태 및 게임 시간 설정
        gameClearPanel.SetActive(false); 
        gameOverPanel.SetActive(false); 
        Time.timeScale = 1f; 
        
        // 초기 점수 및 체력 설정
        currentScore = 0;
        currentHealth = maxHealth;
        
        if (hudManager != null)
        {
            hudManager.UpdateScore(currentScore);
            hudManager.UpdateHealth(currentHealth);
            hudManager.SetGameActive(true); 
        }
    }
    
    // 점수 추가
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
    public void TakeDamage()
    {
        currentHealth = Mathf.Max(0, currentHealth - 1);
        
        if (hudManager != null)
        {
            hudManager.UpdateHealth(currentHealth);
        }
        
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // ---------------------------
    // 🏆 성공 처리 메서드
    // ---------------------------
    public void GameClear()
    {
        if (isGameClear) return; // 중복 호출 방지
        isGameClear = true;        
        StartCoroutine(GameClearCoroutine(10f)); // 10초 딜레이 후 클리어 처리
    }
    
    private IEnumerator GameClearCoroutine(float delay)
    {
        // HUD 비활성화
        if (hudManager != null) hudManager.SetGameActive(false);

        // 10초 동안은 기존 스테이지 BGM 그대로 유지
        yield return new WaitForSeconds(delay);
        
        // 클리어 BGM으로 교체 (있을 경우)
        if (gameClearBGM != null)
        {
            PlayBGM(gameClearBGM, false); // 보통 클리어는 한 번만 재생
        }

        // 클리어 패널 활성화 + 시간 정지
        gameClearPanel.SetActive(true); 
        Time.timeScale = 0f;
    }

    // ---------------------------
    // ❌ 게임 오버 처리
    // ---------------------------
    public void GameOver()
    {
        // 게임 오버 BGM으로 교체
        if (gameOverBGM != null)
        {
            PlayBGM(gameOverBGM, false); // 루프 X
        }

        gameOverPanel.SetActive(true); 
        Time.timeScale = 0f; 
        if (hudManager != null) hudManager.SetGameActive(false); 
    }

    // ---------------------------
    // 씬 전환 & 재시작
    // ---------------------------
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
    
    public void RestartGame()
    {
        Time.timeScale = 1f; 
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // ---------------------------
    // 🎵 공통 BGM 재생 함수
    // ---------------------------
    private void PlayBGM(AudioClip clip, bool loop)
    {
        if (bgmSource == null || clip == null)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }
}
