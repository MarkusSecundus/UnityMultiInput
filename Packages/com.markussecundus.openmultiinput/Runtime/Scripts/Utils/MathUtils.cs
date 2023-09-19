using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace MarkusSecundus.Utils
{
    internal static class MathUtils
    {
        public static Vector2 Clamp(this Vector2 v, Rect boundary)
            => new Vector2(Mathf.Clamp(v.x, boundary.xMin, boundary.xMax), Mathf.Clamp(v.y, boundary.yMin, boundary.yMax));
        public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
            => new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
        public static Vector2 Clamp(this Vector2 v, float min, float max)
            => new Vector2(Mathf.Clamp(v.x, min, max), Mathf.Clamp(v.y, min, max));

        public static Vector2 Size(this Texture2D t) => new Vector2(t.width, t.height);
    }
}