using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Default Cursor")]
    public Texture2D defaultCursorTexture;
    public Vector2 defaultHotSpot = Vector2.zero;

    [Header("Settings")]
    public int cursorSize = 32;
    public CursorMode cursorMode = CursorMode.ForceSoftware;

    // Cache scaled textures to avoid repeated expensive scaling operations
    private Dictionary<string, Texture2D> scaledCache = new Dictionary<string, Texture2D>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (defaultCursorTexture != null)
            ApplyCursor(defaultCursorTexture, defaultHotSpot);
    }

    public void SetDefaultCursor(Texture2D texture, Vector2 hotSpot)
    {
        defaultCursorTexture = texture;
        defaultHotSpot = hotSpot;
        if (texture != null)
            ApplyCursor(texture, hotSpot);
        else
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    public void RestoreDefaultCursor()
    {
        if (defaultCursorTexture != null)
            ApplyCursor(defaultCursorTexture, defaultHotSpot);
        else
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    public void ApplyCursor(Texture2D sourceTexture, Vector2 hotSpot)
    {
        if (sourceTexture == null) return;

        int maxSize = Mathf.Max(8, cursorSize);
        Vector2Int targetSize = GetScaledSize(sourceTexture, maxSize);

        string key = $"{sourceTexture.GetInstanceID()}_{targetSize.x}x{targetSize.y}";
        if (!scaledCache.TryGetValue(key, out var scaled))
        {
            scaled = ScaleTexture(sourceTexture, targetSize.x, targetSize.y);
            scaledCache[key] = scaled;
        }

        Vector2 scaledHotSpot = ScaleHotSpot(sourceTexture, hotSpot, targetSize.x, targetSize.y);
        Cursor.SetCursor(scaled, scaledHotSpot, cursorMode);
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
