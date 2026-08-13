Shader "Unlit/PixelOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}

        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float2 uv      : TEXCOORD0;
                float4 vertex  : SV_POSITION;
                fixed4 color   : COLOR;
            };

            sampler2D _MainTex;

            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            fixed4 _OutlineColor;
            float _Radius;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // This receives SpriteRenderer.color.
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float nearbyAlpha = 0.0;
                int radius = (int)round(_Radius);
                int radiusSquared = radius * radius;

                for (int nx = -radius; nx <= radius; nx++)
                {
                    for (int ny = -radius; ny <= radius; ny++)
                    {
                        // Use radius squared for a circular search area.
                        if ((nx * nx) + (ny * ny) <= radiusSquared)
                        {
                            float2 offset = float2(
                                _MainTex_TexelSize.x * nx,
                                _MainTex_TexelSize.y * ny
                            );

                            fixed sampledAlpha =
                                tex2D(_MainTex, i.uv + offset).a;

                            nearbyAlpha += ceil(sampledAlpha);
                        }
                    }
                }

                nearbyAlpha = saturate(nearbyAlpha);

                fixed4 textureColor = tex2D(_MainTex, i.uv);

                // Apply SpriteRenderer.color to the sprite itself.
                fixed4 spriteColor = textureColor * i.color;

                // Remove the outline from pixels occupied by the sprite.
                float outlineMask =
                    nearbyAlpha - ceil(textureColor.a);

                outlineMask = saturate(outlineMask);

                // Let SpriteRenderer alpha also fade the outline.
                fixed4 outlineColor = _OutlineColor;
                outlineColor.a *= i.color.a;

                return lerp(spriteColor, outlineColor, outlineMask);
            }

            ENDCG
        }
    }
}