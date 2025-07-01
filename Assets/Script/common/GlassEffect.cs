// GlassEffect.cs (挂载到UI元素上)
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GlassEffect : MonoBehaviour {
    public int blurRadius = 5;
    public float brightness = 0.9f;
    public Texture backgroundTexture;
    
    private Material blurMaterial;
    private RawImage rawImage;

    void Start() {
        rawImage = GetComponentInChildren<RawImage>();
        blurMaterial = new Material(Shader.Find("UI/SimpleGlassBlur"));
        rawImage.material = blurMaterial;
    }

    void Update() {
        // 使用屏幕截图作为背景（简化的实现）
        if (backgroundTexture == null || !Application.isPlaying) {
            backgroundTexture = ScreenCapture.CaptureScreen(GetComponent<RectTransform>());
        }

        rawImage.texture = backgroundTexture;
        blurMaterial.SetFloat("_BlurAmount", blurRadius);
        
        // 调整透明度创建磨砂效果
        Color c = rawImage.color;
        c.a = Mathf.Clamp01(brightness);
        rawImage.color = c;
    }
    
    void OnRectTransformDimensionsChange() {
        if (backgroundTexture != null) {
            Destroy(backgroundTexture);
            backgroundTexture = null;
        }
    }
}

// 简单截图工具
public static class ScreenCapture {
    public static Texture2D CaptureScreen(RectTransform rt) {
        Rect rect = RectTransformUtility.PixelAdjustRect(rt, rt.GetComponentInParent<Canvas>());
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        
        RenderTexture rtTemp = RenderTexture.GetTemporary(tex.width, tex.height, 24);
        Camera.main.targetTexture = rtTemp;
        Camera.main.Render();
        
        RenderTexture.active = rtTemp;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rtTemp);
        
        return tex;
    }
}