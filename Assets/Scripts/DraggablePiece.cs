using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 touchOffset;
    private bool isDragging = false;
    private bool isPlaced = false;
    private PieceStackSlot ownerSlot;

    // Týklama ile sürüklemeyi ayýrt etmek için deðiþkenler
    private Vector3 clickStartMousePos;
    private const float DragThreshold = 0.2f; // Bu mesafeden az hareket edilirse "týklama" sayýlýr

    private SpriteRenderer[] childRenderers;
    private Collider2D pieceCollider;

    void Awake()
    {
        pieceCollider = GetComponent<Collider2D>();
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    void Start()
    {
        initialPosition = transform.position;
    }
    public void SetOwnerSlot(PieceStackSlot slot)
    {
        ownerSlot = slot;
    }
    void OnMouseDown()
    {
        if (isPlaced) return;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPick();
        }
        SetRenderOrder(10); // Sürüklerken en öne al

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        clickStartMousePos = mouseWorldPos;
        touchOffset = transform.position - mouseWorldPos + new Vector3(0, 0.8f, 0);
        transform.localScale = Vector3.one; // Tutunca tam boyuta dön
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (!isDragging || isPlaced) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        transform.position = mouseWorldPos + touchOffset;
    }

    void OnMouseUp()
    {
        if (!isDragging || isPlaced) return;

        isDragging = false;
        SetRenderOrder(5);

        Vector3 mouseReleasePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseReleasePos.z = 0f;

        float moveDistance = Vector3.Distance(clickStartMousePos, mouseReleasePos);

        if (moveDistance < DragThreshold)
        {
            RotatePiece();
            if (ownerSlot != null) ownerSlot.OnPieceResetToSlot();
            else transform.position = initialPosition;
            return;
        }

        // Yerleþtirme denemesinde sourceSlot referansýný gönderiyoruz:

        bool placedSuccessfully = GridManager.Instance.TryPlacePiece(transform, ownerSlot);

        if (placedSuccessfully)
        {
            isPlaced = true;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySnap();
            if (pieceCollider != null) pieceCollider.enabled = false;
            if (ownerSlot != null)
            {
                ownerSlot.OnPiecePlacedSuccessfully();
            }
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayFail();
            if (ownerSlot != null)
            {
                ownerSlot.OnPieceResetToSlot();
            }
            else
            {
                transform.position = initialPosition;
                transform.localScale = Vector3.one * 0.7f;
            }
        }
    }

    private void RotatePiece()
    {
        // Parçayý Z ekseninde -90 derece (saat yönünde) çevir

        transform.Rotate(0, 0, -90f);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRotate();
        }
    }

    private void SetRenderOrder(int order)
    {
        foreach (var sr in childRenderers)
        {
            sr.sortingOrder = order;
        }
    }
}