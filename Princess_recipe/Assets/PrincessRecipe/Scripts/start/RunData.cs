using UnityEngine;

public class RunData : MonoBehaviour
{
    public static RunData I { get; private set; }

    public int choice1 = -1;
    public int choice2 = -1;
    public int choice3 = -1;

    private void Awake()
    {
        // 🔴 에디터 상태에서 DontDestroyOnLoad 절대 호출 금지
        if (!Application.isPlaying)
            return;

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
        int c1 = Mathf.Max(0, choice1);
        int c2 = Mathf.Max(0, choice2);
        int c3 = Mathf.Max(0, choice3);
        return c1 + c2 * 2 + c3 * 4;
    }
}
