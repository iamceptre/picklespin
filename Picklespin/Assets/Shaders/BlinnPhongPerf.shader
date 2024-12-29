Shader "Exile/FPSHands/BlinnPhongPerfSmooth"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        _Color("Tint Color", Color) = (1,1,1,1)
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _Shininess("Shininess", Range(1,256)) = 16
        _Glossiness("Glossiness (Softness)", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }
        LOD 200

        CGPROGRAM
        // SKIPPALBLE
        // nolightmap, nodynlightmap -> skip GI
        #pragma surface surf MyBlinnPhong fullforwardshadows nolightmap nodynlightmap
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;

        half4 _Color;
        half4 _SpecularColor;
        half  _Shininess;
        half  _Glossiness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap; 
        };

        //sets Albedo, Normal, etc.
        void surf(Input IN, inout SurfaceOutput o)
        {
            half4 albedoSample = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = albedoSample.rgb;
            o.Alpha  = 1.0;
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
        }

        //Blinn-Phong
        half4 LightingMyBlinnPhong(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half3 N = normalize(s.Normal);
            half3 H = normalize(lightDir + viewDir);

            half diff = saturate(dot(N, lightDir));

            half adjustedShininess = _Shininess * (1.0 + _Glossiness);
            half spec = pow(saturate(dot(N, H)), adjustedShininess);

            half3 color = s.Albedo * diff + _SpecularColor.rgb * spec;
            color *= _LightColor0.rgb * atten;

            return half4(color, s.Alpha);
        }
        ENDCG
    }

    FallBack "Diffuse"
}
