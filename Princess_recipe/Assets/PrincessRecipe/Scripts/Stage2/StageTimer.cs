using UnityEngine;
using TMPro;

public class StageTimer : MonoBehaviour
{
    [Header("Time Text (TMP)")]
    public TextMeshProUGUI timeText;

    private float elapsedTime = 0f;
    private bool isRunning = true;

    void Start()
    {
        if (timeText == null)
            timeText = GetComponent<TextMeshProUGUI>();

        elapsedTime = 0f;
        UpdateTimeText();
    }

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateTimeText();
    }

    void UpdateTimeText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // 외부에서 멈추고 싶을 때 (GameOver / Clear)
    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
