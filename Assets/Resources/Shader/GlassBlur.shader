// GlassBlur.shader
Shader "UI/SimpleGlassBlur" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 3
    }
    
    SubShader {
        Tags { "Queue"="Transparent" }
        
        CGINCLUDE
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        float4 _MainTex_ST;
        float _BlurAmount;
        
        struct v2f {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };
        
        v2f vert(appdata_base v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            return o;
        }
        ENDCG
        
        // 简易高斯模糊
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            fixed4 frag(v2f i) : SV_Target {
                float2 blurSize = float2(_BlurAmount / 800.0, 0);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                col += tex2D(_MainTex, i.uv + float2(blurSize.x, 0));
                col += tex2D(_MainTex, i.uv - float2(blurSize.x, 0));
                col += tex2D(_MainTex, i.uv + float2(0, blurSize.y));
                col += tex2D(_MainTex, i.uv - float2(0, blurSize.y));
                
                return col / 5.0;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}