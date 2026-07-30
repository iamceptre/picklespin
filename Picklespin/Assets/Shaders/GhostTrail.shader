// Additive-looking ghost trail for the BUILT-IN render pipeline.
//
// This project renders with Built-in RP (no SRP asset assigned), so this is
// plain CGPROGRAM/UnityCG — a "RenderPipeline"="UniversalPipeline" SubShader is
// silently skipped here and renders magenta.
//
// Blending is fixed at One/One with BlendOp Max, and is deliberately not
// exposed on the material:
//
//   result = max(trail, background)
//
// On the dark arena that reads as additive — the trail lifts pixels toward its
// own colour. But max() is idempotent, so a pixel covered twice is identical to
// a pixel covered once. The trail brightens up to _Color * _Intensity and stops
// there, no matter how many times it folds over itself. Plain additive
// (One/One with Add) would keep summing and blow out every fold into the bright
// jagged wedges that trace out the triangulation.
//
// Smoothness comes from computing the cross-ribbon profile procedurally with a
// smoothstep, which is C1-continuous — it has no gradient discontinuity at the
// edges or anywhere across a triangle boundary, so the tessellation cannot show
// through. _MainTex is optional and defaults to white; leaving it empty keeps
// the trail perfectly smooth, since a stretched texture is the other thing that
// makes trail geometry visible.
Shader "Picklespin/GhostTrail"
{
    Properties
    {
        [HDR] _Color ("Tint", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity (max brightness)", Range(0, 8)) = 1.5
        _Falloff ("Edge Falloff", Range(0.25, 8)) = 1.5
        _MainTex ("Trail Texture (optional)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        LOD 100

        Pass
        {
            // Blend must come before BlendOp — setting it after resets the op
            Blend One One
            BlendOp Max
            ZWrite Off
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float3 uv    : TEXCOORD0; // xy = tiled UV, z = raw V across the ribbon
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            half _Intensity;
            half _Falloff;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv.z = v.texcoord.y;      // untiled, so the profile ignores tiling
                o.color = v.color * _Color; // vertex colour carries the trail's gradient
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv.xy) * i.color;

                // 0 at both edges, 1 down the centre line
                half across = 1.0h - abs(i.uv.z * 2.0h - 1.0h);

                // smoothstep: flat gradient at both ends, so there is no crease
                // anywhere for the triangulation to show up against
                half profile = across * across * (3.0h - 2.0h * across);
                profile = pow(profile, _Falloff);

                half alpha = c.a * profile;

                // Premultiplied, which is what Max needs: it has no alpha term,
                // so the fade has to live in the colour. alpha 0 -> black ->
                // max() leaves the background exactly as it was.
                return fixed4(c.rgb * _Intensity * alpha, alpha);
            }
            ENDCG
        }
    }

    Fallback "Legacy Shaders/Particles/Alpha Blended"
}
