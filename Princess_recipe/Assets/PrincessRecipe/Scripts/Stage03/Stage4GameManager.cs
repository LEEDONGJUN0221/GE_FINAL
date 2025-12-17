using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Stage4GameManager : MonoBehaviour
{
    public static Stage4GameManager Instance;

    [Header("Player HP")]
    public int maxHP = 3;
    public int currentHP;

    [Header("Vine Pattern")]
    public VinePatternSpawner vineSpawner;

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

    [Header("Clear Delay")]
    public float clearToMapUIDelay = 2.0f; // 클리어 후 MapUI 나오기까지 시간


    // ✅ (선택) 게임오버는 기존대로 ResultPanel 써도 되고, 빼도 됨
    [Header("Result UI (GameOver용 - 원하면 유지)")]
    public GameObject resultPanel;
    public TMP_Text resultText;

    [Header("Restart")]
    public string restartSceneName;

    [Header("Phase Speed (Fruit Count Based)")]
    public float phase1BeatInterval = 2.40f;
    public float phase2BeatInterval = 2.20f;
    public float phase3BeatInterval = 1.90f;

    [Header("Phase Transition: Camera Zoom")]
    public CameraZoomController cameraZoom;

    // ✅ MapUI로 클리어 → 엔딩 씬 이동
    [Header("★ Clear -> MapUI -> Ending")]
    public MapUI mapUI;                 // 씬에 있는 MapUI 연결
    public string endingSceneName = "Epilogue"; // 엔딩/에필로그 씬 이름

    private bool isGameOver = false;
    private bool isGameClear = false;
    private float playTime = 0f;

    private int lastPhase = -1;

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

        // ✅ 인스펙터에서 안 넣었으면 자동 찾기
        if (mapUI == null)
            mapUI = FindAnyObjectByType<MapUI>();
    }

    private IEnumerator Start()
    {
        currentHP = maxHP;

        if (vineSpawner == null)
        {
            Debug.LogError("[Stage4] VinePatternSpawner is NULL (Inspector에 연결 필요)");
            yield break;
        }

        UpdateUI();

        // ✅ 게임 시작 연출: 보스 줌인 → 복귀 → 게임 시작
        if (cameraZoom != null)
        {
            cameraZoom.PlayBossCinematic(1);
            while (cameraZoom.IsPlaying) yield return null;
        }

        StartCoroutine(CoPatternLoop());
    }

    public void Restart()
    {
        // 결과 패널에서 게임 멈춰둔 상태(TimeScale=0) 복구
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


    private void Update()
    {
        if (isGameOver || isGameClear) return;

        playTime += Time.deltaTime;
        UpdateUI();
    }

    private int GetPhaseByFruit()
    {
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
            float progress = (targetFruitCount <= 0)
                ? 0f
                : Mathf.Clamp01((float)currentFruitCount / targetFruitCount);

            vineSpawner.telegraphTime =
                Mathf.Lerp(vineSpawner.telegraphStart, vineSpawner.telegraphMin, progress);

            vineSpawner.activeTime = vineSpawner.activeTimeBase;

            int phase = GetPhaseByFruit();
            if (phase != lastPhase)
            {
                lastPhase = phase;

                if (cameraZoom != null && (phase == 2 || phase == 3))
                {
                    cameraZoom.PlayBossCinematic(phase);
                    while (cameraZoom.IsPlaying) yield return null;
                }
            }

            if (phase == 1) vineSpawner.TriggerRandomLines(2, 3);
            else vineSpawner.TriggerRandomPattern();

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

        UpdateUI();

        if (currentHP <= 0)
            GameOver();
    }

    private void ShowResult(string message)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = message;

        Time.timeScale = 0f;
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();

        // ✅ 게임오버는 기존 ResultPanel 유지
        ShowResult("GAME OVER");
    }

    private void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;

        StopAllCoroutines();
        if (vineSpawner != null) vineSpawner.ClearAllImmediate();

        // ✅ 클리어 연출 후 MapUI 열기
        StartCoroutine(CoClearSequence());
    }

    private IEnumerator CoClearSequence()
    {
        // (선택) 클리어 연출
        if (cameraZoom != null)
        {
            cameraZoom.PlayBossCinematic(99);
            while (cameraZoom.IsPlaying) yield return null;
        }

        // ✅ 여기서 딜레이
        yield return new WaitForSeconds(clearToMapUIDelay);

        // ✅ MapUI 열기 (Stage3는 선택 없음 → 엔딩 버튼만)
        if (mapUI != null)
        {
            mapUI.OpenEnding(endingSceneName);
            // 내부에서 Time.timeScale = 0 처리됨
        }
        else
        {
            Debug.LogError("[Stage3] MapUI 없음!");
            Time.timeScale = 0f;
        }
    }



    // ---------------- Fruit API ----------------
    public bool CanCollectFrom(int treeId)
    {
        return treeId != lastCollectedTreeId;
    }

    public void OnFruitCollected()
    {
        if (isGameOver || isGameClear) return;

        currentFruitCount++;
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
}
