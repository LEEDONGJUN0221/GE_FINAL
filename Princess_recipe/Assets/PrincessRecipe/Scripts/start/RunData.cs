using UnityEngine;

public class RunData : MonoBehaviour
{
    public static RunData I { get; private set; }

    public int choice0 = -1; // Start 선택
    public int choice1 = -1; // Stage1 선택
    public int choice2 = -1; // Stage2 선택

    private void Awake()
    {
        if (!Application.isPlaying) return;

        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public int EndingId()
    {
        int c0 = Mathf.Max(0, choice0);
        int c1 = Mathf.Max(0, choice1);
        int c2 = Mathf.Max(0, choice2);
        return c0 + c1 * 2 + c2 * 4; // 0~7
    }
}
