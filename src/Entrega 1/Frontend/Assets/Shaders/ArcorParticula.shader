// Partículas da reformulação. _Modo: 0 = aditiva (brilhos, faíscas), 1 = suave (fumaça, poeira).
// Usa a cor de cada partícula (cor dos vértices) x textura.
Shader "Arcor/Particula"
{
    Properties
    {
        _BaseMap ("Textura", 2D) = "white" {}
        _Intensidade ("Intensidade", Range(0, 6)) = 1.6
        _BaseColor ("Cor", Color) = (1, 1, 1, 1)
        [Enum(UnityEngine.Rendering.BlendMode)] _Origem ("Blend origem", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _Destino ("Blend destino", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend [_Origem] [_Destino]
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Particula"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Intensidade;
                float4 _BaseColor;
                float _Origem;
                float _Destino;
            CBUFFER_END

            struct Entrada { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Saida { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float fog : TEXCOORD1; };

            Saida Vert(Entrada i)
            {
                Saida o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.color = i.color;
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 Frag(Saida i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                half4 c = t * i.color * _BaseColor;
                c.rgb *= _Intensidade;
                // na aditiva, a transparência vira "quanto soma"
                if (_Destino == 1) c.rgb *= c.a;
                c.rgb = MixFog(c.rgb, i.fog);
                return c;
            }
            ENDHLSL
        }
    }
}
