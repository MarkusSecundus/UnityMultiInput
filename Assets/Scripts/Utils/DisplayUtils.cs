
using UnityEngine;

public static class DisplayUtils
{
    private static Display GetDisplay(this Camera cam) => Display.displays[
#if UNITY_EDITOR
            0
#else
    cam.targetDisplay
#endif
            ];

    public static Vector2 PixelWidthHeight(this Camera cam) => new Vector2(cam.pixelWidth, cam.pixelHeight);

    public static Rect GetScreenRect(this Camera cam)
    {
        var dimsOfWholeDisplay = cam.PixelWidthHeight()/cam.rect.size;

        return new Rect(dimsOfWholeDisplay*cam.rect.position, cam.PixelWidthHeight());
    }
}