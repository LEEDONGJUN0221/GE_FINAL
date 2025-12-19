using UnityEngine;
using UnityEngine.UI;

public class HUDManagerStage3 : MonoBehaviour
{
    [Header("HP 아이콘(최대 HP 개수만큼)")]
    public Image[] hpIcons;   // 화면에 배치된 하트/HP 이미지들 (0번이 첫번째)

    [Header("HP 스프라이트")]
    public Sprite fullSprite;
    public Sprite emptySprite;

    [Header("GameManager 참조")]
    public Stage4GameManager gm;

    private int lastHp = int.MinValue;
    private int lastMaxHp = int.MinValue;

    private void Awake()
    {
        if (gm == null)
            gm = FindAnyObjectByType<Stage4GameManager>();
    }

    private void Start()
    {
        // 필수 체크
        if (hpIcons == null || hpIcons.Length == 0)
        {
            Debug.LogError("[HUDManagerStage3] hpIcons 배열이 비어있습니다.");
            enabled = false;
            return;
        }
        if (fullSprite == null || emptySprite == null)
        {
            Debug.LogError("[HUDManagerStage3] fullSprite/emptySprite가 연결되지 않았습니다.");
            enabled = false;
            return;
        }
        if (gm == null)
        {
            Debug.LogError("[HUDManagerStage3] Stage4GameManager를 찾지 못했습니다. Inspector에 연결하세요.");
            enabled = false;
            return;
        }

        ForceRefresh();
    }

    private void Update()
    {
        if (gm == null) return;

        if (gm.currentHP != lastHp || gm.maxHP != lastMaxHp)
        {
            ApplyHp(gm.currentHP, gm.maxHP);
        }
    }

    public void ForceRefresh()
    {
        if (gm == null) return;
        ApplyHp(gm.currentHP, gm.maxHP);
    }

    private void ApplyHp(int currentHp, int maxHp)
    {
        lastHp = currentHp;
        lastMaxHp = maxHp;

        int iconCount = hpIcons.Length;

        // 표시할 최대치: (maxHp, 아이콘 수) 중 작은 값
        int shownMax = Mathf.Clamp(maxHp, 0, iconCount);
        int clampedHp = Mathf.Clamp(currentHp, 0, shownMax);

        // 1) maxHP 범위까진 켜고, 나머지 아이콘은 숨김(선택)
        for (int i = 0; i < iconCount; i++)
        {
            if (hpIcons[i] == null) continue;
            hpIcons[i].enabled = (i < shownMax);
        }

        // 2) 앞에서부터 clampedHp개는 Full, 나머지는 Empty
        for (int i = 0; i < shownMax; i++)
        {
            if (hpIcons[i] == null) continue;

            hpIcons[i].sprite = (i < clampedHp) ? fullSprite : emptySprite;
        }
    }
}
