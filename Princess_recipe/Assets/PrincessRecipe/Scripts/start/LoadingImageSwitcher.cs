using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingImageSwitcher : MonoBehaviour
{
    [Header("로딩 이미지 2개")]
    public Image imageA;
    public Image imageB;

    [Header("전환 간격 (초)")]
    public float switchInterval = 0.5f;

    private Coroutine routine;

    void OnEnable()
    {
        // 패널 켜질 때 시작
        routine = StartCoroutine(SwitchRoutine());
    }

    void OnDisable()
    {
        // 패널 꺼질 때 정지
        if (routine != null)
            StopCoroutine(routine);
    }

    IEnumerator SwitchRoutine()
    {
        bool showA = true;

        imageA.gameObject.SetActive(true);
        imageB.gameObject.SetActive(false);

        while (true)
        {
            yield return new WaitForSecondsRealtime(switchInterval);

            showA = !showA;
            imageA.gameObject.SetActive(showA);
            imageB.gameObject.SetActive(!showA);
        }
    }
}
