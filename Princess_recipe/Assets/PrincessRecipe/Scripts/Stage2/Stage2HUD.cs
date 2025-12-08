using UnityEngine;
using UnityEngine.UI;

public class Stage2HUD : MonoBehaviour
{
    [Header("참조")]
    public GameManagerStage2 gameManager;   // GameManagerStage2 넣어줄 자리
    public Image[] hpIcons;                 // 딸기 4개 Image
    public Sprite hpFullSprite;             // 멀쩡한 딸기
    public Sprite hpBrokenSprite;           // 깨진 딸기

    private int lastHp = -1;

    void Start()
    {
        // 인스펙터에서 안 넣었으면 자동으로 Instance 사용
        if (gameManager == null)
            gameManager = GameManagerStage2.Instance;

        ForceRefresh();
    }

    void Update()
    {
        if (gameManager == null) return;

        if (gameManager.currentHP != lastHp)
        {
            ForceRefresh();
        }
    }

    void ForceRefresh()
    {
        if (gameManager == null || hpIcons == null || hpIcons.Length == 0)
            return;

        lastHp = gameManager.currentHP;
        int hp = Mathf.Clamp(lastHp, 0, hpIcons.Length);

        for (int i = 0; i < hpIcons.Length; i++)
        {
            if (hpIcons[i] == null) continue;

            hpIcons[i].sprite = (i < hp) ? hpFullSprite : hpBrokenSprite;
        }

        Debug.Log($"[Stage2HUD] 하트 업데이트 - HP: {hp}");
    }
}
