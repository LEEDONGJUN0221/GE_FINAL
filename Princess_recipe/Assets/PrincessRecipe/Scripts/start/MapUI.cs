using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapUI : MonoBehaviour
{
    public GameObject mapPanel;
    public GameObject loadingPanel;   // ★ 추가: 로딩화면 패널
    public MonoBehaviour playerMove;

    public float loadingSeconds = 5f; // ★ 추가: 로딩 유지 시간

    private bool isLoading = false;   // ★ 중복 클릭 방지

    public void CloseMap()
    {
        mapPanel.SetActive(false);

        if (playerMove != null) playerMove.enabled = true;

        Time.timeScale = 1f;
    }

    public void LoadStage(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadStageRoutine(sceneName));
    }

    private IEnumerator LoadStageRoutine(string sceneName)
    {
        isLoading = true;

        // 맵 닫기 + 시간 복구 (중요)
        mapPanel.SetActive(false);
        Time.timeScale = 1f;

        if (playerMove != null) playerMove.enabled = true;

        // 로딩 패널 켜기
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // ★ timeScale 영향 안 받게 리얼타임 대기
        yield return new WaitForSecondsRealtime(loadingSeconds);

        // 씬 이동
        SceneManager.LoadScene(sceneName);
    }
}
