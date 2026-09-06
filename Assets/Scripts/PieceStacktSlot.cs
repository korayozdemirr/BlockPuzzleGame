using UnityEngine;
using TMPro;

public class PieceStackSlot : MonoBehaviour
{
    public GameObject piecePrefab;
    public int count;
    public GameObject badgePrefab;

    private GameObject activePieceInstance;
    private GameObject badgeInstance;
    private TextMeshPro badgeText;

    public void Initialize(GameObject prefab, int initialCount, GameObject badgePref)
    {
        piecePrefab = prefab;
        count = initialCount;
        badgePrefab = badgePref;

        SpawnTopPiece();
        CreateBadge();
        UpdateBadgeText();
    }

    public void SpawnTopPiece()
    {
        if (count <= 0)
        {
            if (badgeInstance != null) badgeInstance.SetActive(false);
            return;
        }

        if (badgeInstance != null) badgeInstance.SetActive(true);

        activePieceInstance = Instantiate(piecePrefab, transform.position, Quaternion.identity);
        activePieceInstance.transform.localScale = Vector3.one * 0.55f;

        DraggablePiece draggable = activePieceInstance.GetComponent<DraggablePiece>();
        if (draggable != null)
        {
            draggable.SetOwnerSlot(this);
        }
    }

    private void CreateBadge()
    {
        if (badgePrefab == null) return;

        Vector3 badgePos = transform.position + new Vector3(0.55f, 0.7f, 0f);
        badgeInstance = Instantiate(badgePrefab, badgePos, Quaternion.identity);
        badgeInstance.transform.SetParent(this.transform);
        badgeText = badgeInstance.GetComponentInChildren<TextMeshPro>();
    }

    public void OnPiecePlacedSuccessfully()
    {
        count--;
        UpdateBadgeText();
        SpawnTopPiece();
    }

    public void RestorePiece()
    {
        // Eðer sahnede hazýr bekleyen aktif bir kopya varsa yok et, yeniden doður
        if (activePieceInstance != null)
        {
            Destroy(activePieceInstance);
        }

        count++;
        UpdateBadgeText();
        SpawnTopPiece();
    }

    public void OnPieceResetToSlot()
    {
        if (activePieceInstance != null)
        {
            activePieceInstance.transform.position = transform.position;
            activePieceInstance.transform.localScale = Vector3.one * 0.55f;
        }
    }

    private void UpdateBadgeText()
    {
        if (badgeText != null)
        {
            badgeText.text = $"x{count}";
        }
    }
}