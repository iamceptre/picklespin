Shader "Custom/InvertUI"
{
    Properties
    {
        _MaskTex("MaskTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        GrabPass{ "_GrabTex" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            sampler2D _GrabTex;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 grabUV : TEXCOORD1;
            };
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MaskTex);
                o.grabUV = ComputeScreenPos(o.vertex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 m = tex2D(_MaskTex, i.uv);
                fixed4 sc = tex2Dproj(_GrabTex, i.grabUV);
                return fixed4(1 - sc.rgb, 1) * m.a;
            }
            ENDCG
        }
    }
}
