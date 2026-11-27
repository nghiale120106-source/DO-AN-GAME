Shader "DuaXe3D/PostFX"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Vignette;
            float _Saturation;
            float _Contrast;
            float _Brightness;
            float _Grain;
            float4 _Tint;

            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float4 c = tex2D(_MainTex, uv);

                float l = dot(c.rgb, float3(0.299, 0.587, 0.114));
                c.rgb = lerp(float3(l, l, l), c.rgb, _Saturation);
                c.rgb = (c.rgb - 0.5) * _Contrast + 0.5;
                c.rgb *= _Tint.rgb;
                c.rgb *= _Brightness;

                float2 d = uv - 0.5;
                float vig = saturate(1.0 - dot(d, d) * _Vignette);
                c.rgb *= vig;

                float n = frac(sin(dot(uv * _Time.xy, float2(12.9898, 78.233))) * 43758.5453);
                c.rgb += (n - 0.5) * _Grain;

                return c;
            }
            ENDCG
        }
    }
    FallBack Off
}
