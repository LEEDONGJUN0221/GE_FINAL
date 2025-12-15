using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapUI : MonoBehaviour
{
    [Header("패널")]
    public GameObject mapPanel;      // 클리어 패널/지도 패널로 사용
    public GameObject loadingPanel;

    [Header("플레이어 이동 잠금(있으면 연결)")]
    public MonoBehaviour playerMove;

    [Header("로딩")]
    public float loadingSeconds = 2f;

    [Header("이번에 저장할 선택 인덱스 (0=Start, 1=Stage1, 2=Stage2)")]
    [Range(0,2)]
    public int choiceIndex = 0;

    [Header("선택 후 이동할 씬")]
    public string nextSceneName = "Stage_01";

    private bool isLoading = false;

    // 외부에서 패널 오픈할 때 호출
    public void OpenChoice(int idx, string nextScene)
    {
        choiceIndex = Mathf.Clamp(idx, 0, 2);
        nextSceneName = nextScene;

        if (mapPanel != null) mapPanel.SetActive(true);

        Time.timeScale = 0f;
        if (playerMove != null) playerMove.enabled = false;
    }

    // A/B 버튼에 연결
    public void ChooseA() => ChooseAndGo(0);
    public void ChooseB() => ChooseAndGo(1);

    private void ChooseAndGo(int value)
    {
        if (isLoading) return;

        SaveChoice(choiceIndex, value);

        // UI 닫기 + 시간 복구
        if (mapPanel != null) mapPanel.SetActive(false);
        Time.timeScale = 1f;
        if (playerMove != null) playerMove.enabled = true;

        StartCoroutine(LoadStageRoutine(nextSceneName));
    }

    private void SaveChoice(int idx, int value)
    {
        if (RunData.I == null)
        {
            Debug.LogError("RunData가 씬에 없음! (StartScene에 RunData 오브젝트 필요)");
            return;
        }

        Debug.Log($"[SAVE] idx={idx}, value={value}, BEFORE c0={RunData.I.choice0}, c1={RunData.I.choice1}, c2={RunData.I.choice2}");

        if (idx == 0) RunData.I.choice0 = value;
        else if (idx == 1) RunData.I.choice1 = value;
        else if (idx == 2) RunData.I.choice2 = value;

        Debug.Log($"[SAVE] AFTER  c0={RunData.I.choice0}, c1={RunData.I.choice1}, c2={RunData.I.choice2}");
    }

    private IEnumerator LoadStageRoutine(string sceneName)
    {
        isLoading = true;

        if (loadingPanel != null) loadingPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(loadingSeconds);

        SceneManager.LoadScene(sceneName);
    }
}
