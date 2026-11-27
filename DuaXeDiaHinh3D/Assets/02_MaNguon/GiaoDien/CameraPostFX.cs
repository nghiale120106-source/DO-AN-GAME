using UnityEngine;

namespace DuaXe3D
{
    /// <summary>
    /// Hiệu ứng hậu kỳ nhẹ: vignette, độ tương phản, độ bão hòa, hạt phim.
    /// Chạy trên Built-in Render Pipeline, không cần package.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraPostFX : MonoBehaviour
    {
        [Range(0f, 2f)] public float vignette = 0.62f;
        [Range(0.5f, 1.6f)] public float saturation = 1.07f;
        [Range(0.5f, 1.6f)] public float contrast = 1.045f;
        [Range(0.5f, 1.6f)] public float brightness = 1.02f;
        [Range(0f, 0.08f)] public float grain = 0.012f;
        public Color tint = Color.white;

        private Material _material;

        private void OnDisable()
        {
            if (_material != null)
            {
                DestroyImmediate(_material);
                _material = null;
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("DuaXe3D/PostFX");
                if (shader != null)
                {
                    _material = new Material(shader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            if (_material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            _material.SetFloat("_Vignette", vignette);
            _material.SetFloat("_Saturation", saturation);
            _material.SetFloat("_Contrast", contrast);
            _material.SetFloat("_Brightness", brightness);
            _material.SetFloat("_Grain", grain);
            _material.SetColor("_Tint", tint);
            Graphics.Blit(source, destination, _material);
        }
    }
}
