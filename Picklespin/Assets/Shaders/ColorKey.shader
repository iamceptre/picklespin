Shader "Unlit/ColorKey"
{
    Properties
    {
        // We keep only the color property and threshold.
        _SelectedColor("Selected Color", Color) = (1,1,1,1)
        _Threshold("Threshold", Range(0,1)) = 0.01
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;       // If you want to use mesh vertex color
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 color    : COLOR;
            };

            float4 _SelectedColor;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.color    = v.color; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Compare the vertex color (or just use i.color if that’s your “pixel color”).
                float3 inColor = i.color.rgb; 

                float d = distance(inColor, _SelectedColor.rgb);
                if(d > _Threshold)
                    discard;

                // Or just return the vertex color (or any color you want).
                return float4(inColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback "Unlit/Transparent"
}
