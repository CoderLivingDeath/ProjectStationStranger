using UnityEngine;
using UnityEngine.UIElements;

public class RatioVisualElement : VisualElement
{
    [UxmlAttribute("width")]
    public int RatioWidth
    {
        get => _ratioWidth;
        set
        {
            _ratioWidth = value;
            UpdateAspect();
        }
    }

    // The ratio of height.
    [UxmlAttribute("height")]
    public int RatioHeight
    {
        get => _ratioHeight;
        set
        {
            _ratioHeight = value;
            UpdateAspect();
        }
    }

    // Padding elements to keep the aspect ratio.
    private int _ratioWidth = 16;
    private int _ratioHeight = 9;

    public RatioVisualElement()
    {
        // Update the padding elements when the geometry changes.
        RegisterCallback<GeometryChangedEvent>(UpdateAspectAfterEvent);
        // Update the padding elements when the element is attached to a panel.
        RegisterCallback<AttachToPanelEvent>(UpdateAspectAfterEvent);
    }

    static void UpdateAspectAfterEvent(EventBase evt)
    {
        var element = evt.target as RatioVisualElement;
        element?.UpdateAspect();
    }

    private void ClearPadding()
    {
        style.paddingLeft = 0;
        style.paddingRight = 0;
        style.paddingBottom = 0;
        style.paddingTop = 0;
    }

    // Update the padding.
    private void UpdateAspect()
    {
        var designRatio = (float)RatioWidth / RatioHeight;
        var currRatio = resolvedStyle.width / resolvedStyle.height;
        var diff = currRatio - designRatio;

        if (RatioWidth <= 0.0f || RatioHeight <= 0.0f)
        {
            ClearPadding();
            Debug.LogError($"[AspectRatio] Invalid width:{RatioWidth} or height:{RatioHeight}");
            return;
        }

        if (float.IsNaN(resolvedStyle.width) || float.IsNaN(resolvedStyle.height))
        {
            return;
        }

        if (diff > 0.01f)
        {
            var w = (resolvedStyle.width - (resolvedStyle.height * designRatio)) * 0.5f;
            style.paddingLeft = w;
            style.paddingRight = w;
            style.paddingTop = 0;
            style.paddingBottom = 0;
        }
        else if (diff < -0.01f)
        {
            var h = (resolvedStyle.height - (resolvedStyle.width * (1 / designRatio))) * 0.5f;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.paddingTop = h;
            style.paddingBottom = h;
        }
        else
        {
            ClearPadding();
        }
    }
}
