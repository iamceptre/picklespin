Shader "Custom/ProceduralClouds"
{
    Properties
    {
        _CloudColor("Cloud Color (RGBA)", Color) = (1,1,1,1)
        _SkyColor("Sky Color", Color) = (0.4,0.6,1,1)
        _WindDirection("Wind Direction (XY)", Vector) = (1,0,0,0)
        _WindSpeed("Wind Speed", Range(0,0.2)) = 0.05
        _Scale("Cloud Scale", Range(0.1,20.0)) = 5
        _Density("Cloud Density", Range(0,1)) = 0.55
        _FractalLayers("Fractal Detail", Range(1,6)) = 4
        _Parallax("Base Parallax Depth", Range(0,1)) = 0.3
        _EdgeFade("Edge Fade", Range(0,0.5)) = 0.1
        _MorphIntensity("Morph Intensity", Range(0,5)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define GOLDEN_RATIO 1.618
            #define HASH_SHIFT float2(127.1, 311.7)
            #define HASH_MULT 43758.5453

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvCloud : TEXCOORD0;
                float2 uvOriginal : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            fixed4 _CloudColor;
            fixed4 _SkyColor;
            float4 _WindDirection;
            float _WindSpeed;
            float _Scale;
            float _Density;
            float _FractalLayers;
            float _Parallax;
            float _EdgeFade;
            float _MorphIntensity;

            // Simple hash for pseudo-random
            float hash(float2 p)
            {
                return frac(sin(dot(p, HASH_SHIFT)) * HASH_MULT);
            }

            // Basic value noise
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) 
                     + (c - a) * u.y * (1.0 - u.x) 
                     + (d - b) * u.x * u.y;
            }

            // Fractal Brownian Motion
            float fbm(float2 uv)
            {
                float sum = 0;
                float amp = 0.5;
                float freq = 1.0;

                int maxLayers = (int)_FractalLayers;
                for (int i = 0; i < maxLayers; i++)
                {
                    sum += noise(uv * freq) * amp;
                    freq *= GOLDEN_RATIO;
                    amp *= 0.5;
                }
                return sum;
            }

            // Generates coverage (cloud mask)
            float coverage(float2 uv, float density, float parallax, float timeY)
            {
                // Precompute offsets to avoid repeating multiplications
                float timeOffset = timeY * 0.1;

                // Morph offsets
                float2 w = float2(
                    fbm((uv + timeOffset) * GOLDEN_RATIO),
                    fbm((uv - timeOffset) * GOLDEN_RATIO)
                ) - 0.5;

                uv += w * _MorphIntensity;

                float c1 = fbm(uv);
                float c2 = fbm(uv * 1.2) * parallax;

                float cloudVal = (c1 + c2) * 0.5;

                return smoothstep(density, density + 0.15, cloudVal);
            }

            v2f vert(appdata_t v)
            {
                v2f o;

                // Wind direction + speed
                float2 d = normalize(_WindDirection.xy);
                float timeOffset = _Time.y * _WindSpeed;

                // Shift UV by wind over time
                o.uvCloud = (v.uv + d * timeOffset) * _Scale;

                o.uvOriginal = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Cache sine calls to avoid repeating them
                float sinTime31 = sin(_Time.y * 0.31);
                float sinTime23 = sin(_Time.y * 0.23);

                // Slight variation in density and parallax over time
                float dp = saturate(_Parallax + 0.05 * sinTime31);
                float dd = saturate(_Density  + 0.02 * sinTime23);

                // Compute cloud coverage
                float c = coverage(i.uvCloud, dd, dp, _Time.y);

                // Extra small FBM for alpha variation
                float alphaNoise = fbm(i.uvCloud * 2.73);
                float alphaVary  = saturate(0.7 + alphaNoise * 0.3);

                // Lerp sky/cloud color
                fixed4 col = lerp(_SkyColor, _CloudColor, c);

                // Final alpha from coverage, per-pixel noise, and user RGBA alpha
                col.a = c * alphaVary * _CloudColor.a;

                // Edge fade based on distance to border of mesh UV
                float2 u = i.uvOriginal;
                float borderDist = min(min(u.x, 1.0 - u.x), min(u.y, 1.0 - u.y));
                float e = smoothstep(0.0, _EdgeFade, borderDist);
                col.a *= e;

                return col;
            }
            ENDCG
        }
    }
}
