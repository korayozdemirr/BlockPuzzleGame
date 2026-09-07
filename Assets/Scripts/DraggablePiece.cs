using UnityEngine;

public class DraggablePiece : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 touchOffset;
    private bool isDragging = false;
    private bool isPlaced = false;
    private PieceStackSlot ownerSlot;

    // T�klama ve S�r�kleme Ayr�m�
    private Vector3 clickStartMousePos;
    private float clickStartTime;
    private bool hasTriggeredPick = false;
    private const float DragThreshold = 0.25f; // Bir miktar hareket edince s�r�kleme ba�lar
    private const float ClickDurationLimit = 0.3f; // Bu s�reden k�sa ve hareketsizse kesin t�klamad�r

    private SpriteRenderer[] childRenderers;
    private Collider2D pieceCollider;

    void Awake()
    {
        pieceCollider = GetComponent<Collider2D>();
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
        CenterChildrenPivot(); // Pivot kaymas�n� otomatik �nleyen sihirli metot
    }

    void Start()
    {
        initialPosition = transform.position;
    }

    public void SetOwnerSlot(PieceStackSlot slot)
    {
        ownerSlot = slot;
    }

    // Par�a i�indeki kutular�n merkezini (0,0,0) noktas�na toplayan kod
    private void CenterChildrenPivot()
    {
        if (transform.childCount == 0) return;

        Vector3 centerOffset = Vector3.zero;
        foreach (Transform child in transform)
        {
            centerOffset += child.localPosition;
        }
        centerOffset /= transform.childCount;

        // Kareleri pivot merkezine �ek
        foreach (Transform child in transform)
        {
            child.localPosition -= centerOffset;
        }
    }

    void OnMouseDown()
    {
        if (isPlaced) return;
        if (Input.GetMouseButtonDown(1)) return; // Sa� t�k filtresi

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        clickStartMousePos = mouseWorldPos;
        clickStartTime = Time.time;
        hasTriggeredPick = false;
        isDragging = false;

        // Parma��n par�ay� kapatmamas� i�in hafif yukar� offset
        touchOffset = transform.position - mouseWorldPos + new Vector3(0, 0.8f, 0f);
    }

    void OnMouseDrag()
    {
        if (isPlaced) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        float distance = Vector3.Distance(clickStartMousePos, mouseWorldPos);

        // E�er parmak e�ikten fazla kayd�ysa s�r�kleme aktifle�ir
        if (!isDragging && distance > DragThreshold)
        {
            isDragging = true;

            // S�r�kleme ba�lad��� an SADECE B�R KEZ pick sesi �alar
            if (!hasTriggeredPick)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayPick();
                hasTriggeredPick = true;
            }

            SetRenderOrder(10); // S�r�klerken �ne al
            transform.localScale = Vector3.one; // Tam boyuta ge�
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

        // 1. TIKLAMA / D�ND�RME KONTROL�
        // E�er s�r�kleme ba�lamad�ysa veya �ok az hareket edip k�sa s�rede b�rakt�ysa:
        if (!isDragging && moveDistance < DragThreshold && holdDuration < ClickDurationLimit)
        {
            RotatePiece();
            ResetToHomePosition();
            return;
        }

        // 2. S�R�KLEME VE YERLE�T�RME KONTROL�
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
        // Par�ay� kendi etraf�nda saat y�n�nde 90 derece �evir
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