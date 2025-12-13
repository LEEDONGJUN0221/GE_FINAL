using UnityEngine;

public class TelegraphBlink : MonoBehaviour
{
    public float blinkSpeed = 10f;
    public float alphaMin = 0.2f;
    public float alphaMax = 0.8f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (sr == null) return;

        float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color c = sr.color;
        c.a = Mathf.Lerp(alphaMin, alphaMax, t);
        sr.color = c;
    }
}
