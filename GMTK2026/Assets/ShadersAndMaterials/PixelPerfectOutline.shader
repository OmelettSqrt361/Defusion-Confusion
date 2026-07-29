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
            float4 _MainTex_TexelSize; // x = 1/width, y = 1/height

            fixed4 _OutlineColor;
            int _OutlineThickness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                // Clean standard vertex projection (no vertex distortion)
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

                // If the pixel is already solid, draw the sprite normally
                if (c.a > 0.1)
                {
                    return c;
                }

                int radius = clamp(_OutlineThickness, 1, 10);
                float alphaMax = 0.0;

                // Sample surrounding pixels using texel size
                [loop]
                for (int x = -10; x <= 10; x++)
                {
                    if (abs(x) > radius) continue;

                    [loop]
                    for (int y = -10; y <= 10; y++)
                    {
                        if (abs(y) > radius || (x == 0 && y == 0)) continue;

                        float2 offset = float2(x, y) * _MainTex_TexelSize.xy;
                        float2 nUV = IN.texcoord + offset;

                        fixed4 neighbor = tex2D(_MainTex, nUV);
                        alphaMax = max(alphaMax, neighbor.a);
                    }
                }

                // If neighbor pixels are opaque, render the outline color here
                if (alphaMax > 0.1)
                {
                    fixed4 outline = _OutlineColor;
                    outline.a *= alphaMax;
                    return outline;
                }

                return c;
            }
            ENDCG
        }
    }
}