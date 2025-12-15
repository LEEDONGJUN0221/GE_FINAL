using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro; // TMP 쓰면 유지, 안 쓰면 지워도 됨
using UnityEngine.SceneManagement;
public class Stage4GameManager : MonoBehaviour
{
    public static Stage4GameManager Instance;

    [Header("Player HP")]
    public int maxHP = 3;
    public int currentHP;

    [Header("Vine Pattern")]
    public VinePatternSpawner vineSpawner;
    public float beatInterval = 1.2f;

    [Header("Fruit Progress")]
    public int targetFruitCount = 16;
    public int currentFruitCount = 0;

    [Header("UI (둘 중 하나만 연결해도 됨)")]
    public Text hpText;
    public Text fruitText;
    public Text timeText;

    public TMP_Text hpTMP;
    public TMP_Text fruitTMP;
    public TMP_Text timeTMP;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public string restartSceneName;
    private bool isGameOver = false;
    private bool isGameClear = false;
    private float playTime = 0f;

    [Header("Phase Speed (Fruit Count Based)")]
    public float phase1BeatInterval = 2.40f;
    public float phase2BeatInterval = 2.20f;
    public float phase3BeatInterval = 1.90f;

    [Header("Phase Transition FX")]
    public float phasePauseDuration = 0.35f;   // 페이즈 전환 시 멈춤 시간
    public float shakeDuration = 0.25f;        // 카메라 흔들림 시간
    public float shakeMagnitude = 0.20f;       // 흔들림 크기(월드 단위)

    private int lastPhase = -1;                // 직전 페이즈 기록
    private Vector3 camBasePos;                // 카메라 원래 위치


    // 같은 나무 연속 수확 금지
    private int lastCollectedTreeId = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentHP = maxHP;

        if (vineSpawner == null)
        {
            Debug.LogError("[Stage4] VinePatternSpawner is NULL (Inspector에 연결 필요)");
            return;
        }

        UpdateUI();
        StartCoroutine(CoPatternLoop());
        camBasePos = Camera.main.transform.position;

    }

    private void Update()
    {
        if (isGameOver || isGameClear) return;

        playTime += Time.deltaTime;
        UpdateUI();
    }

    private int GetPhaseByFruit()
    {
        // 0-5개: 페이즈1, 5-12: 페이즈2, 12-16: 페이즈3
        // 경계 포함 처리를 명확히 하기 위해 다음처럼 정리:
        // 0~4 = 1, 5~11 = 2, 12~ = 3
        if (currentFruitCount >= 12) return 3;
        if (currentFruitCount >= 5) return 2;
        return 1;
    }

    private float GetBeatIntervalByPhase(int phase)
    {
        switch (phase)
        {
            case 1: return phase1BeatInterval;
            case 2: return phase2BeatInterval;
            default: return phase3BeatInterval;
        }
    }

    private IEnumerator CoPatternLoop()
    {
        yield return new WaitForSeconds(1.0f);

        while (!isGameOver && !isGameClear)
        {
            // 1) 과일 진행도 기준 경고시간 계산 (기존 유지)
            float progress = (targetFruitCount <= 0)
                ? 0f
                : Mathf.Clamp01((float)currentFruitCount / targetFruitCount);

            vineSpawner.telegraphTime =
                Mathf.Lerp(vineSpawner.telegraphStart, vineSpawner.telegraphMin, progress);

            vineSpawner.activeTime = vineSpawner.activeTimeBase;

            // ✅ 2) 페이즈 먼저 계산
            int phase = GetPhaseByFruit();
            if (phase != lastPhase)
            {
                lastPhase = phase;
                yield return StartCoroutine(CoOnPhaseChanged(phase));
            }
            // ✅ 3) 패턴 발동 (Phase1만 신규, Phase2/3는 기존)
            if (phase == 1)
            {
                // (VinePatternSpawner에 TriggerRandomLines(min,max) 있어야 함)
                vineSpawner.TriggerRandomLines(2, 3);
            }
            else
            {
                // Phase2/3: 기존 홀/짝 행/열 4패턴 랜덤
                vineSpawner.TriggerRandomPattern();
            }

            // 4) 페이즈별 속도 계산
            float interval = GetBeatIntervalByPhase(phase);
            yield return new WaitForSeconds(interval);
        }
    }



    

    // ---------------- Damage ----------------
    public void TakeDamage(int damage)
    {
        if (isGameOver || isGameClear) return;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"[Stage4] HP {currentHP}/{maxHP}");
        UpdateUI();

        if (currentHP <= 0)
            GameOver();
    }
    private void ShowResult(string message){
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = message;

            // (선택) 게임 정지
            Time.timeScale = 0f; 
        }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();
        
        
        ShowResult("GAME OVER");

        Debug.Log("[Stage4] GAME OVER");
        // TODO: 게임오버 UI 표시/씬 이동
    }

    private void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;
        
        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();
        
        
        Debug.Log("[Stage4] GAME CLEAR");
        ShowResult("CLEAR");
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        // restartSceneName 비어있으면 현재 씬 재시작
        if (string.IsNullOrEmpty(restartSceneName))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(restartSceneName);
        }
    }


    // ---------------- Fruit API (FruitTree.cs 호환 유지) ----------------
    public bool CanCollectFrom(int treeId)
    {
        return treeId != lastCollectedTreeId;
    }

    public void OnFruitCollected()
    {
        if (isGameOver || isGameClear) return;

        currentFruitCount++;
        Debug.Log($"[Stage4] Fruit {currentFruitCount}/{targetFruitCount}");
        UpdateUI();

        if (currentFruitCount >= targetFruitCount)
            GameClear();
    }

    public void OnCollectedFromTree(int treeId)
    {
        lastCollectedTreeId = treeId;
    }

    // ---------------- UI ----------------
    private void UpdateUI()
    {
        string hpStr = $"HP: {currentHP}/{maxHP}";
        string fruitStr = $"Fruit: {currentFruitCount}/{targetFruitCount}";
        string timeStr = $"Time: {playTime:0.0}s";

        if (hpText) hpText.text = hpStr;
        if (fruitText) fruitText.text = fruitStr;
        if (timeText) timeText.text = timeStr;

        if (hpTMP) hpTMP.text = hpStr;
        if (fruitTMP) fruitTMP.text = fruitStr;
        if (timeTMP) timeTMP.text = timeStr;
    }

    private IEnumerator CoCameraShakeRealtime(float duration, float magnitude)
    {
        if (Camera.main == null) yield break;

        Transform cam = Camera.main.transform;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float offsetX = Random.Range(-magnitude, magnitude);
            float offsetY = Random.Range(-magnitude, magnitude);

            cam.position = camBasePos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        cam.position = camBasePos;
    }


    private IEnumerator CoOnPhaseChanged(int newPhase)
    {
        // 1) 잠깐 멈춤
        Time.timeScale = 0f;

        // 2) 흔들림 (Realtime로 돌아가야 멈춘 상태에서도 동작함)
        yield return StartCoroutine(CoCameraShakeRealtime(shakeDuration, shakeMagnitude));

        // 3) 멈춤 유지 시간 (원하면 shake랑 별개로 더 줄 수도 있음)
        yield return new WaitForSecondsRealtime(phasePauseDuration);

        // 4) 재개
        Time.timeScale = 1f;

        // (선택) 페이즈 로그
        Debug.Log($"[Stage4] Phase Changed => {newPhase}");
    }



}
