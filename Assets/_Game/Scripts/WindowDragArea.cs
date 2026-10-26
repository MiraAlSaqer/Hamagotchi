using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler
{
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    private int dragOffsetX;
    private int dragOffsetY;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Focus handling if needed
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (GetCursorPos(out POINT cursor))
        {
            if (DesktopWindowManager.Instance.GetCurrentWindowPosition(out int winX, out int winY))
            {
                dragOffsetX = cursor.X - winX;
                dragOffsetY = cursor.Y - winY;
                DesktopWindowManager.Instance.UpdateWorkAreaLimits();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (GetCursorPos(out POINT cursor))
        {
            int targetX = cursor.X - dragOffsetX;
            int targetY = cursor.Y - dragOffsetY;

            DesktopWindowManager.Instance.MoveWindowClamped(targetX, targetY);
        }
    }
}