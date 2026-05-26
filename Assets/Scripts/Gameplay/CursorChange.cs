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

    void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
    }

    // These two work for UI buttons and Sprites!
    //  Must need: 
    //   Buttons to have Raycast Target checked
    //   2D Sprites need a 2D Collider
    //   Scene Camera needs a Phsyics 2D Raycaster
    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(pointerTexture, pointerHotSpot, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorTexture, cursorHotSpot, CursorMode.ForceSoftware);
    }


}