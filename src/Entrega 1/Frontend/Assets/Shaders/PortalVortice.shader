// Interior do portal: espiral em faixas chapadas, animada só pelo tempo, sem precisar de script.
Shader "Arcor/Portal Vortice"
{
    Properties
    {
        [HDR] _CorFundo ("Cor do fundo", Color) = (0.05, 0.0, 0.2, 1)
        [HDR] _CorFaixa ("Cor das faixas", Color) = (1.6, 0.05, 1.5, 1)
        [HDR] _CorBorda ("Cor da borda", Color) = (0.05, 2.6, 3.4, 1)
        [HDR] _CorCentro ("Cor do centro", Color) = (5, 9, 11, 1)
        _Bracos ("Braços", Range(1, 8)) = 4
        _Torcao ("Torção", Range(0, 8)) = 3.2
        _Velocidade ("Velocidade", Range(-3, 3)) = 0.45
        _Degraus ("Degraus de cor", Range(2, 8)) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _CorFundo;
                float4 _CorFaixa;
                float4 _CorBorda;
                float4 _CorCentro;
                float _Bracos;
                float _Torcao;
                float _Velocidade;
                float _Degraus;
            CBUFFER_END

            struct Entrada
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Saida
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Saida Vert(Entrada i)
            {
                Saida o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv * 2.0 - 1.0;
                return o;
            }

            half4 Frag(Saida i) : SV_Target
            {
                float r = length(i.uv);
                float a = atan2(i.uv.y, i.uv.x) / (2.0 * PI);

                // Espiral em poucos degraus de cor, para combinar com as facetas do jogo
                float s = frac(a * _Bracos + r * _Torcao - _Time.y * _Velocidade);
                float onda = abs(s * 2.0 - 1.0);
                float faixa = floor(onda * _Degraus) / (_Degraus - 1.0);

                float3 cor = lerp(_CorFundo.rgb, _CorFaixa.rgb, faixa);
                cor = lerp(cor, _CorBorda.rgb, step(0.9, r));
                cor = lerp(cor, _CorCentro.rgb, step(r, 0.22 + 0.03 * sin(_Time.y * 2.5)));
                return half4(cor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 Vert(float4 p : POSITION) : SV_POSITION { return TransformObjectToHClip(p.xyz); }
            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
