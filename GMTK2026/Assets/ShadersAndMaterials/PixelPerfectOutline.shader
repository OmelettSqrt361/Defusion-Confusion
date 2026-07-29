Shader "Sprites/PixelPerfectOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        [HideInInspector] _OutlineThickness ("Outline Thickness (Pixels)", Int) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _OutlineColor;
            int _OutlineThickness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // 1. If pixel is already solid sprite artwork, return normal pixel
                if (c.a > 0.1)
                {
                    return c;
                }

                int radius = clamp(_OutlineThickness, 1, 10);

                // FIX: use (radius + 0.5) as the growth boundary instead of `radius`.
                // A strict Euclidean circle (dist <= radius) under-includes diagonal
                // pixels on an integer grid -- e.g. at radius=1 it drops the (1,1)
                // corner entirely, and at radius=2 it drops near-corner pixels like
                // (2,1). That's what produced the notched/uneven look. Testing against
                // the pixel's outer edge (radius + 0.5) instead matches how selection
                // "grow" tools like GIMP's actually dilate a mask -- full 3x3 fill at
                // radius 1, smooth octagon-like growth at larger radii.
                float radiusF = (float)radius + 0.5;
                float radiusSqF = radiusF * radiusF;

                float maxAlpha = 0.0;

                // 2. Circular (Euclidean) distance sampling
                [loop]
                for (int x = -10; x <= 10; x++)
                {
                    if (abs(x) > radius) continue;

                    [loop]
                    for (int y = -10; y <= 10; y++)
                    {
                        if (abs(y) > radius || (x == 0 && y == 0)) continue;

                        // Skip samples outside the (corrected) circular radius so the
                        // outline grows as a smooth disc rather than squaring off,
                        // while still fully filling small radii like GIMP's grow does.
                        if ((float)(x * x + y * y) > radiusSqF) continue;

                        float2 offset = float2(x, y) * _MainTex_TexelSize.xy;
                        float2 sampleUV = IN.texcoord + offset;

                        // Keep samples within 0 to 1 UV range
                        if (sampleUV.x >= 0.0 && sampleUV.x <= 1.0 && sampleUV.y >= 0.0 && sampleUV.y <= 1.0)
                        {
                            fixed4 neighbor = tex2D(_MainTex, sampleUV);

                            if (neighbor.a > 0.1)
                            {
                                maxAlpha = max(maxAlpha, neighbor.a);
                            }
                        }
                    }
                }

                // 3. Render outline color following the rounded silhouette
                if (maxAlpha > 0.1)
                {
                    fixed4 outline = _OutlineColor;
                    outline.a *= maxAlpha;
                    return outline;
                }

                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
