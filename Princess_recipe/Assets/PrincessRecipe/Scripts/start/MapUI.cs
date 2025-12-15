using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject mapPanel;       // (Start에서는 지도 UI, Stage에서는 클리어 패널로 써도 됨)
    public GameObject loadingPanel;
    public MonoBehaviour playerMove;

    [Header("로딩")]
    public float loadingSeconds = 5f;

    [Header("선택 정보 (디버그/기본값)")]
    public int choiceIndex = 1;                 // 1=Start→Stage1, 2=Stage1→Stage2, 3=Stage2→Stage3
    public string nextSceneName = "Stage_01";   // 이동할 씬

    private bool isLoading = false;

    // -------------------------
    // ★ 외부(지도 상호작용 / 보스 클리어)에서 호출해서 패널 열기
    // -------------------------
    public void OpenChoice(int idx, string nextScene)
    {
        choiceIndex = idx;
        nextSceneName = nextScene;

        if (mapPanel != null) mapPanel.SetActive(true);

        // 선택 중 멈춤
        Time.timeScale = 0f;
        if (playerMove != null) playerMove.enabled = false;
    }

    // -------------------------
    // A / B 버튼에 직접 연결
    // -------------------------
    public void ChooseA() => ChooseAndGo(0);
    public void ChooseB() => ChooseAndGo(1);

    private void ChooseAndGo(int value)
    {
        if (isLoading) return;

        // 1) 선택 저장
        SaveChoice(choiceIndex, value);

        // 2) UI 정리 + 시간 복구
        if (mapPanel != null) mapPanel.SetActive(false);
        Time.timeScale = 1f;
        if (playerMove != null) playerMove.enabled = true;

        // 3) 로딩 후 씬 이동
        StartCoroutine(LoadStageRoutine(nextSceneName));
    }

    private void SaveChoice(int idx, int value)
    {
        if (RunData.I == null)
        {
            Debug.LogError("RunData가 씬에 없음! (StartScene에 RunData 오브젝트 필요)");
            return;
        }

        if (idx == 1) RunData.I.choice1 = value;
        else if (idx == 2) RunData.I.choice2 = value;
        else if (idx == 3) RunData.I.choice3 = value;

        Debug.Log($"[CHOICE SAVED] idx={idx}, value={value}  (c1={RunData.I.choice1}, c2={RunData.I.choice2}, c3={RunData.I.choice3})");
    }

    private IEnumerator LoadStageRoutine(string sceneName)
    {
        isLoading = true;

        if (loadingPanel != null) loadingPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(loadingSeconds);

        SceneManager.LoadScene(sceneName);
    }
}
