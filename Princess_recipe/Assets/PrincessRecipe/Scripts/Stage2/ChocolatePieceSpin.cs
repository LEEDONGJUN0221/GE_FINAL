using UnityEngine;
using System.Collections;

public class ChocolatePieceSpin : MonoBehaviour
{
    [Header("Flip 설정")]
    public float flipInterval = 0.2f; // 뒤집히는 속도 (작을수록 빠름)

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        StartCoroutine(FlipRoutine());
    }

    IEnumerator FlipRoutine()
    {
        while (true)
        {
            if (sr != null)
                sr.flipY = !sr.flipY;

            yield return new WaitForSeconds(flipInterval);
        }
    }
}
