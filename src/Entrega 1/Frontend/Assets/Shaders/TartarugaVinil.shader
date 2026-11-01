// Material "vinil" da tartaruga: cor vinda dos vértices (já com sombreado de cavidades),
// brilho PBR do URP, luz de recorte nas bordas e um preenchimento para não escurecer em cenas com pouca luz.
Shader "Arcor/Tartaruga Vinil"
{
    Properties
    {
        _BaseMap ("Textura", 2D) = "white" {}
        _Smoothness ("Brilho", Range(0, 1)) = 0.65
        _Preenchimento ("Preenchimento", Range(0, 1)) = 0.3
        _CorRecorte ("Cor do recorte", Color) = (1, 0.9, 0.75, 1)
        _ForcaRecorte ("Força do recorte", Range(0, 2)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Smoothness;
                float _Preenchimento;
                float4 _CorRecorte;
                float _ForcaRecorte;
            CBUFFER_END

            struct Entrada
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Saida
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 color : COLOR;
                float fog : TEXCOORD3;
            };

            Saida Vert(Entrada i)
            {
                Saida o;
                VertexPositionInputs pos = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.color = i.color;
                o.fog = ComputeFogFactor(pos.positionCS.z);
                return o;
            }

            half4 Frag(Saida i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 v = GetWorldSpaceNormalizeViewDir(i.positionWS);

                // A cor dos vértices é gravada em sRGB
                half3 corVertice = i.color.rgb;
                #if !defined(UNITY_COLORSPACE_GAMMA)
                corVertice = SRGBToLinear(corVertice);
                #endif
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * corVertice;

                InputData dados = (InputData)0;
                dados.positionWS = i.positionWS;
                dados.positionCS = i.positionCS;
                dados.normalWS = n;
                dados.viewDirectionWS = v;
                dados.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                dados.fogCoord = i.fog;
                dados.bakedGI = SampleSH(n);
                dados.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                dados.shadowMask = half4(1, 1, 1, 1);

                SurfaceData sup = (SurfaceData)0;
                sup.albedo = albedo;
                sup.specular = half3(0, 0, 0);
                sup.metallic = 0;
                sup.smoothness = _Smoothness;
                sup.normalTS = half3(0, 0, 1);
                sup.occlusion = 1;
                sup.alpha = 1;

                half4 cor = UniversalFragmentPBR(dados, sup);

                // Preenchimento vindo de cima, como a luz de estúdio das artes de referência
                cor.rgb += albedo * _Preenchimento * (0.6 + 0.4 * n.y);

                // Recorte nas bordas para destacar a silhueta
                float fresnel = pow(saturate(1.0 - dot(n, v)), 3.0);
                cor.rgb += _CorRecorte.rgb * albedo * fresnel * _ForcaRecorte;

                cor.rgb = MixFog(cor.rgb, i.fog);
                cor.a = 1;
                return cor;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
    }

    FallBack "Universal Render Pipeline/Lit"
}
