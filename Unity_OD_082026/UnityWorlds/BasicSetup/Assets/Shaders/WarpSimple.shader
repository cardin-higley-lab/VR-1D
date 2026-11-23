Shader "Custom/WarpSimple"
{
    Properties
    {
        _MainTex ("RenderTexture", 2D) = "white" {}
        _WarpX ("Warp Strength X", Float) = 0.15
        _WarpY ("Warp Strength Y", Float) = 0.10
        _FlipX ("Flip Horizontally", Float) = 0
        _FlipY ("Flip Vertically", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Off
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); 
            SAMPLER(sampler_MainTex);

            float _WarpX;
            float _WarpY;
            float _FlipX;
            float _FlipY;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                float2 uv = v.uv;

                // Flip before warp
                if (_FlipX > 0.5) uv.x = 1.0 - uv.x;
                if (_FlipY > 0.5) uv.y = 1.0 - uv.y;

                o.uv = uv;
                return o;
            }

            float2 ParabolicWarp(float2 uv)
            {
                float2 p = uv - 0.5;

                p.x += p.x * abs(p.y) * _WarpX;
                p.y += p.y * abs(p.x) * _WarpY;

                float2 outUV = p + 0.5;

                // Clamp to avoid white borders
                outUV = clamp(outUV, 0.001, 0.999);

                return outUV;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 warpedUV = ParabolicWarp(i.uv);
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, warpedUV);
            }

            ENDHLSL
        }
    }
}
