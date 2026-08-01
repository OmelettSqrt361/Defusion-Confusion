Shader "Sprites/Wobble"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        [Header(Wobble)]
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

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                // Apply wobble displacement using Wobble.cginc
                float4 localVertex = ApplyWobble(IN.vertex, IN.texcoord);

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
                // Standard sprite texture lookup
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                return c;
            }
            ENDCG
        }
    }
}