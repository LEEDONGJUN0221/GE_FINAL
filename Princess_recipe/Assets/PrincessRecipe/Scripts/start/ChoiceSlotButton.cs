using UnityEngine;
using UnityEngine.UI;

public class ChoiceSlotButton : MonoBehaviour
{
    [Header("이 슬롯이 담당하는 선택 번호 (1~3)")]
    public int slotIndex = 1;

    [Header("이번 패널에서 활성화할 선택 번호")]
    public int currentChoiceIndex = 1;

    [Header("A/B 버튼 연결")]
    public Button buttonA;
    public Button buttonB;

    void Start()
    {
        bool isActiveSlot = (slotIndex == currentChoiceIndex);

        // 버튼 활성/비활성
        buttonA.interactable = isActiveSlot;
        buttonB.interactable = isActiveSlot;

        // 이전 슬롯은 버튼 비활성 + 시각적으로 회색 처리
        if (!isActiveSlot)
        {
            SetDim(buttonA);
            SetDim(buttonB);
        }
    }

    private void SetDim(Button btn)
    {
        if (btn == null) return;
        var colors = btn.colors;
        colors.normalColor = new Color(1,1,1,0.3f);
        btn.colors = colors;
    }
}
