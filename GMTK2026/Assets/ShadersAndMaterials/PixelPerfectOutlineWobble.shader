Shader "Sprites/PixelPerfectOutlineWobble"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        [HideInInspector] _OutlineThickness ("Outline Thickness (Pixels)", Int) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        [Header(Wobble)]
        [Toggle] _DoShader ("Do Shader", Float) = 1
        _WobbleAmountPx ("Wobble Amount (texels)", Range(0, 4)) = 0.4
        _WobbleFPS ("Wobble Frame Rate", Range(1, 24)) = 3
        _PixelsPerUnit ("Pixels Per Unit", Float) = 16
        _WobbleFrequency ("Wobble Noise Frequency", Range(1, 100)) = 10
        _WobbleProbability ("Wobble Probability", Range(0, 1)) = 0.25
        [Toggle] _WobbleSnapToPixel ("Snap Wobble To Whole Pixel", Float) = 0
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
            #include "Wobble.cginc"

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
            float _DoShader;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                // Wobble is applied here, in the vertex stage, before anything
                // else -- including before OUT.vertex exists. The outline pass
                // in frag() only ever measures distances in texel/UV space
                // (_MainTex_TexelSize), never vertex position, so it can't
                // "see" the wobble at all. That's what keeps outline thickness
                // constant regardless of how the wobble moves this vertex.
                // When the effect is toggled off, the vertex passes through
                // unmodified.
                float4 localVertex = IN.vertex;
                if (_DoShader > 0.5)
                {
                    localVertex = ApplyWobble(IN.vertex, IN.texcoord);
                }

                OUT.vertex = UnityObjectToClipPos(localVertex);
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

                // If the effect is toggled off, skip the outline pass
                // entirely and render a plain sprite.
                if (_DoShader < 0.5)
                {
                    return c;
                }

                // 1. If pixel is already solid sprite artwork, return normal pixel
                if (c.a > 0.1)
                {
                    return c;
                }

                int radius = clamp(_OutlineThickness, 1, 10);
                float radiusF = (float)radius + 0.5;
                float radiusSqF = radiusF * radiusF;

                float maxAlpha = 0.0;

                // 2. Circular (Euclidean) distance sampling -- unchanged. It only
                //    reads _MainTex UVs relative to the current fragment, so it
                //    automatically re-traces around whatever shape the wobbled
                //    quad is drawing this frame.
                [loop]
                for (int x = -10; x <= 10; x++)
                {
                    if (abs(x) > radius) continue;

                    [loop]
                    for (int y = -10; y <= 10; y++)
                    {
                        if (abs(y) > radius || (x == 0 && y == 0)) continue;

                        if ((float)(x * x + y * y) > radiusSqF) continue;

                        float2 offset = float2(x, y) * _MainTex_TexelSize.xy;
                        float2 sampleUV = IN.texcoord + offset;

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
