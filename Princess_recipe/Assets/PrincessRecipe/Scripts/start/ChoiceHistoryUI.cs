using UnityEngine;
using UnityEngine.UI;

public class ChoiceHistoryUI : MonoBehaviour
{
    [Header("이번 패널에서 몇 개까지 보여줄지 (Stage1=1, Stage2=2, Stage3=3)")]
    [Range(0, 3)]
    public int showCount = 0; // Start씬이면 0도 가능

    [Header("표시 슬롯 (1~3) - 실제 UI 위치에 맞게 연결")]
    public Image slot1;
    public Image slot2;
    public Image slot3;

    [Header("선택 안됨/X 표시(각 슬롯 위 오브젝트)")]
    public GameObject x1;
    public GameObject x2;
    public GameObject x3;

    [Header("슬롯1용 A/B 스프라이트 (Start 선택 표시용)")]
    public Sprite slot1_A;
    public Sprite slot1_B;

    [Header("슬롯2용 A/B 스프라이트 (Stage1 선택 표시용)")]
    public Sprite slot2_A;
    public Sprite slot2_B;

    [Header("슬롯3용 A/B 스프라이트 (Stage2 선택 표시용)")]
    public Sprite slot3_A;
    public Sprite slot3_B;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        // RunData: choice1=Start, choice2=Stage1, choice3=Stage2
        int c1 = RunData.I != null ? RunData.I.choice0 : -1;
        int c2 = RunData.I != null ? RunData.I.choice1 : -1;
        int c3 = RunData.I != null ? RunData.I.choice2 : -1;

        ApplySlot(slot1, x1, c1, slot1_A, slot1_B, showCount >= 1);
        ApplySlot(slot2, x2, c2, slot2_A, slot2_B, showCount >= 2);
        ApplySlot(slot3, x3, c3, slot3_A, slot3_B, showCount >= 3);
    }

    private void ApplySlot(Image slotImg, GameObject xObj, int choice, Sprite A, Sprite B, bool visible)
    {
        if (slotImg == null) return;

        slotImg.gameObject.SetActive(visible);
        if (xObj != null) xObj.SetActive(false);

        if (!visible) return;

        // 선택 안 했으면 X
        if (choice != 0 && choice != 1)
        {
            slotImg.sprite = null;
            slotImg.color = new Color(1, 1, 1, 0); // 투명
            if (xObj != null) xObj.SetActive(true);
            return;
        }

        // 선택 했으면 해당 슬롯의 A/B 스프라이트
        slotImg.sprite = (choice == 0) ? A : B;
        slotImg.color = Color.white;
    }
}
