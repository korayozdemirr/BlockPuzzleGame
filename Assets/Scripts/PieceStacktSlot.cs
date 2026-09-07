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

        CreateBadge();
        SpawnTopPiece();
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

        ApplyGemSprites(activePieceInstance);
    }

    private void ApplyGemSprites(GameObject pieceObj)
    {
        if (pieceObj == null) return;
        SpriteRenderer[] renderers = pieceObj.GetComponentsInChildren<SpriteRenderer>();
        if (renderers == null || renderers.Length == 0) return;

        int blockCount = renderers.Length;
        Sprite targetGemSprite = null;

        if (blockCount == 4)
        {
            targetGemSprite = Resources.Load<Sprite>("Sprites/gem_green_g");
        }
        else if (blockCount == 2)
        {
            targetGemSprite = Resources.Load<Sprite>("Sprites/gem_purple_p");
        }
        else if (blockCount == 3)
        {
            targetGemSprite = Resources.Load<Sprite>("Sprites/gem_blue_b");
        }
        else
        {
            targetGemSprite = Resources.Load<Sprite>("Sprites/gem_green_g");
        }

        if (targetGemSprite != null)
        {
            Vector2 spriteSize = targetGemSprite.bounds.size;
            float scaleX = (spriteSize.x > 0) ? 1.0f / spriteSize.x : 1.0f;
            float scaleY = (spriteSize.y > 0) ? 1.0f / spriteSize.y : 1.0f;

            foreach (var sr in renderers)
            {
                if (sr != null)
                {
                    sr.sprite = targetGemSprite;
                    sr.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }
        }
    }

    private void CreateBadge()
    {
        if (badgeInstance != null) return;

        Vector3 badgePos = transform.position + new Vector3(0.55f, 0.65f, -0.5f);
        if (badgePrefab != null)
        {
            badgeInstance = Instantiate(badgePrefab, badgePos, Quaternion.identity);
            badgeInstance.transform.SetParent(this.transform);
            badgeText = badgeInstance.GetComponentInChildren<TextMeshPro>();
        }
        else
        {
            badgeInstance = new GameObject("Badge_Count");
            badgeInstance.transform.SetParent(this.transform);
            badgeInstance.transform.position = badgePos;

            SpriteRenderer sr = badgeInstance.AddComponent<SpriteRenderer>();
            Sprite badgeSprite = Resources.Load<Sprite>("Sprites/badge_count_red");
            if (badgeSprite != null)
            {
                sr.sprite = badgeSprite;
                Vector2 sSize = badgeSprite.bounds.size;
                float targetBadgeSize = 0.5f;
                float sX = sSize.x > 0 ? targetBadgeSize / sSize.x : 0.45f;
                float sY = sSize.y > 0 ? targetBadgeSize / sSize.y : 0.45f;
                sr.transform.localScale = new Vector3(sX, sY, 1f);
            }
            sr.sortingOrder = 15;

            GameObject textObj = new GameObject("Badge_Text");
            textObj.transform.SetParent(badgeInstance.transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);

            badgeText = textObj.AddComponent<TextMeshPro>();
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.fontSize = 4.5f;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.color = new Color(0.98f, 0.88f, 0.45f); // Gold text
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/LilitaOne-Regular SDF");
            if (font != null) badgeText.font = font;
            badgeText.sortingOrder = 16;
        }
    }

    public void OnPiecePlacedSuccessfully()
    {
        count--;
        UpdateBadgeText();
        SpawnTopPiece();
    }

    public void RestorePiece()
    {
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
        if (badgeInstance != null)
        {
            badgeInstance.SetActive(count > 0);
        }
    }
}