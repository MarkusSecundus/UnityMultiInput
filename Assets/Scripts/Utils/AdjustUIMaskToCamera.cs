using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdjustUIMaskToCamera : MonoBehaviour
{
    public Camera Cam;
    public Mask Mask;

    private Canvas _canvas;
    private Image _maskImage;
    private void Start()
    {
        _canvas = Mask.GetComponentInParent<Canvas>();
        _maskImage = Mask.GetComponent<Image>();
    }
    void LateUpdate()
    {
        _canvas.targetDisplay = Cam.targetDisplay;
        var rectToSet = new Rect(_canvas.pixelRect.width * Cam.rect.x, _canvas.pixelRect.height * Cam.rect.y, _canvas.pixelRect.width * Cam.rect.width, _canvas.pixelRect.height * Cam.rect.height);
    }
}
