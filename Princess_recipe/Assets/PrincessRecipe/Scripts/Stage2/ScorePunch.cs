using UnityEngine;
using System.Collections;

public class ScorePunch : MonoBehaviour
{
    public float punchScale = 1.2f;
    public float duration = 0.15f;

    RectTransform rect;
    Vector3 originalScale;
    Coroutine routine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void Punch()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PunchRoutine());
    }

    IEnumerator PunchRoutine()
    {
        rect.localScale = originalScale * punchScale;
        yield return new WaitForSeconds(duration);
        rect.localScale = originalScale;
    }
}
