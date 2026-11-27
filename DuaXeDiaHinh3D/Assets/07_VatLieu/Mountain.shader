Shader "DuaXe3D/Mountain"
{
    Properties
    {
        _Color ("Rock Color", Color) = (0.45, 0.44, 0.42, 1)
        _SnowColor ("Snow Color", Color) = (0.92, 0.95, 1, 1)
        _SnowHeight ("Snow Height", Float) = 9999
        _SnowBlend ("Snow Blend", Float) = 40
        _Glossiness ("Smoothness", Range(0, 1)) = 0.12
        _Metallic ("Metallic", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        fixed4 _Color;
        fixed4 _SnowColor;
        float _SnowHeight;
        float _SnowBlend;
        half _Glossiness;
        half _Metallic;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float snow = smoothstep(_SnowHeight - _SnowBlend, _SnowHeight + _SnowBlend, IN.worldPos.y);
            snow *= saturate(IN.worldNormal.y * 1.35 - 0.12);
            o.Albedo = lerp(_Color.rgb, _SnowColor.rgb, snow);
            o.Metallic = _Metallic;
            o.Smoothness = lerp(_Glossiness, 0.58, snow);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
