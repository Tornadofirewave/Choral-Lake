using UnityEngine;
using UnityEngine.EventSystems;

public class CursorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cursor Textures")]
    public Texture2D cursorTexture;
    public Texture2D pointerTexture;

    [Header("Cursor Hot Spots")]
    public Vector2 cursorHotSpot = Vector2.zero;
    public Vector2 pointerHotSpot = new Vector2(16, 0);

    [Header("Cursor Size")]
    [Range(8, 128)]
    public int cursorSize = 32;

    void Start()
    {
        ApplyCursor(cursorTexture, cursorHotSpot);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyCursor(pointerTexture, pointerHotSpot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyCursor(cursorTexture, cursorHotSpot);
    }

    void ApplyCursor(Texture2D sourceTexture, Vector2 hotSpot)
    {
        if (sourceTexture == null)
        {
            return;
        }

        int maxSize = Mathf.Max(8, cursorSize);
        
        Vector2Int targetSize = GetScaledSize(sourceTexture, maxSize);
        Texture2D scaled = ScaleTexture(sourceTexture, targetSize.x, targetSize.y);
        Vector2 scaledHotSpot = ScaleHotSpot(sourceTexture, hotSpot, targetSize.x, targetSize.y);

        Cursor.SetCursor(scaled, scaledHotSpot, CursorMode.ForceSoftware);
    }

    Vector2Int GetScaledSize(Texture2D source, int maxSize)
    {
        float scale = maxSize / (float)Mathf.Max(source.width, source.height);
        int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
        int height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));

        return new Vector2Int(width, height);
    }

    Texture2D ScaleTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }

    Vector2 ScaleHotSpot(Texture2D source, Vector2 hotSpot, int targetWidth, int targetHeight)
    {
        float xRatio = targetWidth / (float)source.width;
        float yRatio = targetHeight / (float)source.height;
        return new Vector2(hotSpot.x * xRatio, hotSpot.y * yRatio);
    }
}