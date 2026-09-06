using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 touchOffset;
    private bool isDragging = false;
    private bool isPlaced = false;
    private PieceStackSlot ownerSlot;

    // Týklama ve Sürükleme Ayrýmý
    private Vector3 clickStartMousePos;
    private float clickStartTime;
    private bool hasTriggeredPick = false;
    private const float DragThreshold = 0.25f; // Bir miktar hareket edince sürükleme baþlar
    private const float ClickDurationLimit = 0.3f; // Bu süreden kýsa ve hareketsizse kesin týklamadýr

    private SpriteRenderer[] childRenderers;
    private Collider2D pieceCollider;

    void Awake()
    {
        pieceCollider = GetComponent<Collider2D>();
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
        CenterChildrenPivot(); // Pivot kaymasýný otomatik önleyen sihirli metot
    }

    void Start()
    {
        initialPosition = transform.position;
    }

    public void SetOwnerSlot(PieceStackSlot slot)
    {
        ownerSlot = slot;
    }

    // Parça içindeki kutularýn merkezini (0,0,0) noktasýna toplayan kod
    private void CenterChildrenPivot()
    {
        if (transform.childCount == 0) return;

        Vector3 centerOffset = Vector3.zero;
        foreach (Transform child in transform)
        {
            centerOffset += child.localPosition;
        }
        centerOffset /= transform.childCount;

        // Kareleri pivot merkezine çek
        foreach (Transform child in transform)
        {
            child.localPosition -= centerOffset;
        }
    }

    void OnMouseDown()
    {
        if (isPlaced) return;
        if (Input.GetMouseButtonDown(1)) return; // Sað týk filtresi

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        clickStartMousePos = mouseWorldPos;
        clickStartTime = Time.time;
        hasTriggeredPick = false;
        isDragging = false;

        // Parmaðýn parçayý kapatmamasý için hafif yukarý offset
        touchOffset = transform.position - mouseWorldPos + new Vector3(0, 0.8f, 0f);
    }

    void OnMouseDrag()
    {
        if (isPlaced) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        float distance = Vector3.Distance(clickStartMousePos, mouseWorldPos);

        // Eðer parmak eþikten fazla kaydýysa sürükleme aktifleþir
        if (!isDragging && distance > DragThreshold)
        {
            isDragging = true;

            // Sürükleme baþladýðý an SADECE BÝR KEZ pick sesi çalar
            if (!hasTriggeredPick)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayPick();
                hasTriggeredPick = true;
            }

            SetRenderOrder(10); // Sürüklerken öne al
            transform.localScale = Vector3.one; // Tam boyuta geç
        }

        if (isDragging)
        {
            transform.position = mouseWorldPos + touchOffset;
        }
    }

    void OnMouseUp()
    {
        if (isPlaced) return;

        Vector3 mouseReleasePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseReleasePos.z = 0f;

        float moveDistance = Vector3.Distance(clickStartMousePos, mouseReleasePos);
        float holdDuration = Time.time - clickStartTime;

        // 1. TIKLAMA / DÖNDÜRME KONTROLÜ
        // Eðer sürükleme baþlamadýysa veya çok az hareket edip kýsa sürede býraktýysa:
        if (!isDragging && moveDistance < DragThreshold && holdDuration < ClickDurationLimit)
        {
            RotatePiece();
            ResetToHomePosition();
            return;
        }

        // 2. SÜRÜKLEME VE YERLEÞTÝRME KONTROLÜ
        if (isDragging)
        {
            isDragging = false;
            SetRenderOrder(5);

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
                ResetToHomePosition();
            }
        }
        else
        {
            ResetToHomePosition();
        }
    }

    private void ResetToHomePosition()
    {
        isDragging = false;
        SetRenderOrder(5);

        if (ownerSlot != null)
        {
            ownerSlot.OnPieceResetToSlot();
        }
        else
        {
            transform.position = initialPosition;
            transform.localScale = Vector3.one * 0.55f;
        }
    }

    private void RotatePiece()
    {
        // Parçayý kendi etrafýnda saat yönünde 90 derece çevir
        transform.Rotate(0, 0, -90f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRotate();
        }
    }

    private void SetRenderOrder(int order)
    {
        if (childRenderers == null) return;
        foreach (var sr in childRenderers)
        {
            if (sr != null) sr.sortingOrder = order;
        }
    }
}