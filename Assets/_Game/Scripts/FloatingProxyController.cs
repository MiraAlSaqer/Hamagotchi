using UnityEngine;

public class FloatingProxyController : MonoBehaviour
{
    private ShopItem sourceItem;
    private Canvas canvas;
    private RectTransform rectTransform;
    private RectTransform hamaRect;
    private Camera uiCamera;

    public void Setup(ShopItem source, Canvas linkedCanvas)
    {
        sourceItem = source;
        canvas = linkedCanvas;
        rectTransform = GetComponent<RectTransform>();

        GameObject hama = GameObject.FindWithTag("Pet");
        hamaRect = hama.GetComponent<RectTransform>();

        uiCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rectTransform.position = mousePos;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                uiCamera,
                out Vector2 localPoint
            );
            rectTransform.localPosition = localPoint;
        }

        if (Input.GetMouseButtonDown(1))
        {
            sourceItem.OnCancelledRefund();
            Destroy(gameObject);
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            TryApplyItem(mousePos);
        }
    }

    private void TryApplyItem(Vector2 screenPosition)
    {
        bool hitHama = RectTransformUtility.RectangleContainsScreenPoint(hamaRect, screenPosition, uiCamera);

        if (!hitHama)
        {
            Vector2 proxyCenterScreen = RectTransformUtility.WorldToScreenPoint(uiCamera, rectTransform.position);
            hitHama = RectTransformUtility.RectangleContainsScreenPoint(hamaRect, proxyCenterScreen, uiCamera);
        }

        if (hitHama)
        {
            sourceItem.OnAppliedSuccess();
        }
        else
        {
            sourceItem.OnCancelledRefund();
        }

        Destroy(gameObject);
    }
}