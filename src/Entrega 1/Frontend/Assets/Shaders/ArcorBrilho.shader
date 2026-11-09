// Partes que brilham (gemas, olhos, lâmpadas): cor dos vértices x intensidade, sem luz.
// Com o Bloom do URP, valores acima de 1 viram brilho. Aceita o _CorPiscar dos scripts.
Shader "Arcor/Brilho Vertice"
{
    Properties
    {
        _Intensidade ("Intensidade", Range(0, 8)) = 2.2
        _CorPiscar ("Cor do pisca (alpha = força)", Color) = (1, 1, 1, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Intensidade;
                float4 _CorPiscar;
            CBUFFER_END

            struct Entrada { float4 positionOS : POSITION; float4 color : COLOR; };
            struct Saida { float4 positionCS : SV_POSITION; float4 color : COLOR; float fog : TEXCOORD0; };

            Saida Vert(Entrada i)
            {
                Saida o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.color = i.color;
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 Frag(Saida i) : SV_Target
            {
                half3 c = i.color.rgb;
                #if !defined(UNITY_COLORSPACE_GAMMA)
                c = SRGBToLinear(c);
                #endif
                c *= _Intensidade;
                c = lerp(c, _CorPiscar.rgb * _Intensidade, _CorPiscar.a);
                c = MixFog(c, i.fog * 0.5);
                return half4(c, 1);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Unlit/DEPTHONLY"
    }

    FallBack "Universal Render Pipeline/Unlit"
}
