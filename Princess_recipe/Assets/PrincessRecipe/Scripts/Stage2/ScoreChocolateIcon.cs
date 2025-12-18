using UnityEngine;
using UnityEngine.UI;

public class ScoreChocolateIcon : MonoBehaviour
{
    [Header("Theme Sprites")]
    public Sprite whiteChocolateIcon;
    public Sprite darkChocolateIcon;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        ApplyThemeFromRunData();
    }

    void ApplyThemeFromRunData()
    {
        if (image == null) return;

        int choice = (RunData.I != null) ? RunData.I.choice1 : 1; // 기본 다크
        bool isWhite = (choice == 0);

        image.sprite = isWhite ? whiteChocolateIcon : darkChocolateIcon;
    }
}
