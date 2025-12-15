using UnityEngine;
using UnityEngine.UI;

public class ChoiceHistoryUI : MonoBehaviour
{
    [Header("이번 패널에서 몇 개까지 보여줄지 (Stage1=1, Stage2=2, Stage3=3)")]
    [Range(1, 3)]
    public int showCount = 1;

    [Header("표시 슬롯 (1~3)")]
    public Image slot1;
    public Image slot2;
    public Image slot3;

    [Header("선택 안됨/X 표시")]
    public GameObject x1;
    public GameObject x2;
    public GameObject x3;

    [Header("선택 A/B 스프라이트")]
    public Sprite spriteA;
    public Sprite spriteB;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int c1 = RunData.I != null ? RunData.I.choice1 : -1;
        int c2 = RunData.I != null ? RunData.I.choice2 : -1;
        int c3 = RunData.I != null ? RunData.I.choice3 : -1;

        ApplySlot(c1, slot1, x1, showCount >= 1);
        ApplySlot(c2, slot2, x2, showCount >= 2);
        ApplySlot(c3, slot3, x3, showCount >= 3);
    }

    private void ApplySlot(int choice, Image slot, GameObject xObj, bool visible)
    {
        if (slot == null)
        {
            Debug.LogError("ChoiceHistoryUI: slot Image가 연결되지 않음", this);
            return;
        }

        // 슬롯 자체 표시/비표시
        slot.gameObject.SetActive(visible);

        if (xObj != null)
            xObj.SetActive(false);

        if (!visible)
            return;

        // 아직 선택 안 함 → X 표시
        if (choice != 0 && choice != 1)
        {
            slot.sprite = null;
            slot.color = new Color(1, 1, 1, 0);

            if (xObj != null)
                xObj.SetActive(true);

            return;
        }

        // 선택됨 → A/B 표시
        slot.sprite = (choice == 0) ? spriteA : spriteB;
        slot.color = Color.white;
    }
}
