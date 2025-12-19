using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stage2ScoreUI : MonoBehaviour
{
    [Header("Theme Sprites")]
    public Sprite whiteIcon;
    public Sprite darkIcon;

    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI text;
    public ScorePunch punch;

    void Start()
    {
        ApplyThemeFromRunData();
    }

    void ApplyThemeFromRunData()
    {
        int choice = (RunData.I != null) ? RunData.I.choice1 : 1;
        bool isWhite = (choice == 0);

        if (icon != null)
            icon.sprite = isWhite ? whiteIcon : darkIcon;
    }

    public void UpdateScore(int cur, int max)
    {
        if (text != null)
            text.text = $"{cur} / {max}";

        if (punch != null)
            punch.Punch();
    }
}
