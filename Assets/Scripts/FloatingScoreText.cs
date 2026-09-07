using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingScoreText : MonoBehaviour
{
    private TextMeshPro tmpText;

    public static void Spawn(Vector3 worldPos, string textContent, Color textColor, float fontSize = 4.5f)
    {
        GameObject go = new GameObject("FloatingScoreText");
        go.transform.position = worldPos + new Vector3(0, 0.3f, 0);

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();

        // Apply LilitaOne casual game font if available
        TMP_FontAsset gameFont = Resources.Load<TMP_FontAsset>("Fonts/LilitaOne-Regular SDF");
        if (gameFont != null)
        {
            tmp.font = gameFont;
        }

        tmp.text = textContent;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        // Sorting Order: Rules state dragged is 10, placed is 2. We render popups at 20 (on top).
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingOrder = 20;
        }

        FloatingScoreText anim = go.AddComponent<FloatingScoreText>();
        anim.tmpText = tmp;
        anim.StartCoroutine(anim.AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float duration = 0.75f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0f, 1.2f, 0f);

        Color startColor = tmpText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float moveT = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(startPos, endPos, moveT);

            float scale = 1f;
            if (t < 0.2f)
            {
                scale = Mathf.Lerp(0.3f, 1.25f, t / 0.2f);
            }
            else if (t < 0.35f)
            {
                scale = Mathf.Lerp(1.25f, 1.0f, (t - 0.2f) / 0.15f);
            }
            transform.localScale = Vector3.one * scale;

            if (t > 0.5f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
                tmpText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
