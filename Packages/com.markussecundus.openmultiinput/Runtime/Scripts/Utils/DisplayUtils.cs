
using UnityEngine;

namespace MarkusSecundus.Utils
{
    internal static class DisplayUtils
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
            var dimsOfWholeDisplay = cam.PixelWidthHeight() / cam.rect.size;

            return new Rect(dimsOfWholeDisplay * cam.rect.position, cam.PixelWidthHeight());
        }

        public static int FindCameraIndex(this Camera cam)
        {
            var allCameras = Camera.allCameras;
            for (int t = 0; t < allCameras.Length; ++t)
                if (allCameras[t].Equals(cam))
                    return t;
            return -1;
        }
        public static int FindCameraIndexOrDefault(this Camera cam, int defaultValue = default)
        {
            var ret = cam.FindCameraIndex();
            return ret >= 0 ? ret : defaultValue;
        }

        public struct SafeCameraBinding
        {
            private int _cameraIndex;
            private Camera _Camera;
            public Camera Value
            {
                get => _Camera ? _Camera : _Camera = Camera.allCameras[System.Math.Min(_cameraIndex, Camera.allCamerasCount)];
                set => _cameraIndex = (_Camera = value).FindCameraIndexOrDefault(0);
            }
        }
    }
}