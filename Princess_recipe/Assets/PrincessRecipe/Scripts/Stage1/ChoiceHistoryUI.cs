using UnityEngine;
using UnityEngine.UI;

public class ChoiceHistoryUI : MonoBehaviour
{
    [Header("이전 선택 표시(이미지)")]
    public Image prevAImage;
    public Image prevBImage;

    [Header("선택 안된 쪽에 띄울 X 오버레이")]
    public GameObject xOverlayA;
    public GameObject xOverlayB;

    [Header("이번 패널에서 참고할 이전 선택 인덱스")]
    public int prevChoiceIndex = 1; // Stage2 전이면 1, Stage3 전이면 2

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int prev = GetChoice(prevChoiceIndex); // 0=A, 1=B, -1=없음

        // 아직 이전 선택이 없으면 둘 다 X 끄기 (원하면 둘다 X 켜도 됨)
        if (prev == -1)
        {
            if (xOverlayA) xOverlayA.SetActive(false);
            if (xOverlayB) xOverlayB.SetActive(false);
            return;
        }

        // 선택된 쪽은 X 끄고, 선택 안된 쪽은 X 켜기
        if (xOverlayA) xOverlayA.SetActive(prev != 0);
        if (xOverlayB) xOverlayB.SetActive(prev != 1);

        // (옵션) 선택된 쪽 강조(밝게), 선택 안된 쪽 흐리게
        if (prevAImage) prevAImage.color = (prev == 0) ? Color.white : new Color(1,1,1,0.35f);
        if (prevBImage) prevBImage.color = (prev == 1) ? Color.white : new Color(1,1,1,0.35f);
    }

    private int GetChoice(int idx)
    {
        if (RunData.I == null) return -1;
        if (idx == 1) return RunData.I.choice1;
        if (idx == 2) return RunData.I.choice2;
        if (idx == 3) return RunData.I.choice3;
        return -1;
    }
}
