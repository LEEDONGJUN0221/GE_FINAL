using UnityEngine;

public class MapInteract : MonoBehaviour
{
    public GameObject mapPanel;      // MapPanel 드래그
    public MonoBehaviour playerMove; // 플레이어 이동 스크립트 드래그 (예: GridMovement)

    private bool playerInRange;
    private bool mapOpen = false;    // 지도 열림 상태

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (mapOpen)
                CloseMap();
            else
                OpenMap();
        }
    }

    private void OpenMap()
    {
        mapPanel.SetActive(true);
        mapOpen = true;

        if (playerMove != null)
            playerMove.enabled = false;

        Time.timeScale = 0f;
    }

    private void CloseMap()
    {
        mapPanel.SetActive(false);
        mapOpen = false;

        if (playerMove != null)
            playerMove.enabled = true;

        Time.timeScale = 1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
