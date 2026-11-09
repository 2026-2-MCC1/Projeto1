// Sombra redonda do jogador (disco com textura suave). _BaseColor.a controla a força.
Shader "Arcor/Sombra"
{
    Properties
    {
        _BaseMap ("Textura", 2D) = "white" {}
        _BaseColor ("Cor", Color) = (0, 0, 0, 0.5)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-50" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Offset -1, -1

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
            CBUFFER_END

            struct Entrada { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Saida { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Saida Vert(Entrada i)
            {
                Saida o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                return o;
            }

            half4 Frag(Saida i) : SV_Target
            {
                half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).a;
                return half4(_BaseColor.rgb, a * _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
