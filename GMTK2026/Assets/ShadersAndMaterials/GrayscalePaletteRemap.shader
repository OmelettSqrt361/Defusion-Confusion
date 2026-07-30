Shader "Custom/GrayscalePaletteRemap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t 
            { 
                float4 vertex : POSITION; 
                float4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
            };

            struct v2f 
            { 
                float4 vertex : SV_POSITION; 
                fixed4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
            };

            sampler2D _MainTex;
            fixed4 _Color;

            // Arrays for dynamic color replacement (up to 16 slots)
            int _MappingCount;
            float4 _SourceColors[16];
            float4 _TargetColors[16];
            float _Tolerances[16];

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, IN.texcoord);
                fixed4 finalCol = texCol;

                for (int i = 0; i < _MappingCount; i++)
                {
                    float dist = distance(texCol.rgb, _SourceColors[i].rgb);
                    if (dist <= _Tolerances[i])
                    {
                        finalCol.rgb = _TargetColors[i].rgb;
                        break;
                    }
                }

                // Apply vertex tint & alpha
                finalCol.a *= IN.color.a;
                finalCol.rgb *= IN.color.rgb;

                // Premultiply — REQUIRED for `Blend One OneMinusSrcAlpha`.
                // Without this, semi-transparent edge pixels contribute too much
                // of their own RGB relative to their alpha, producing fringing/outline
                // artifacts wherever the sprite's alpha isn't fully 0 or 1 (i.e. at
                // anti-aliased edges — exactly the seams you're seeing between tiles).
                finalCol.rgb *= finalCol.a;

                return finalCol;
            }
        ENDCG
        }
    }
}