// Adesivos e rótulos (logos dos mobs, face do selo): textura com recorte por alfa e luz simples,
// no mesmo tom das peças de vinil. Aceita o _CorPiscar dos scripts (o mob pisca inteiro ao apanhar).
Shader "Arcor/Adesivo"
{
    Properties
    {
        _BaseMap ("Textura", 2D) = "white" {}
        _BaseColor ("Cor", Color) = (1, 1, 1, 1)
        _Cutoff ("Recorte", Range(0, 1)) = 0.5
        _Brilho ("Brilho especular", Range(0, 1)) = 0.35
        _CorPiscar ("Cor do pisca (alpha = força)", Color) = (1, 1, 1, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Adesivo"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
                float _Brilho;
                float4 _CorPiscar;
            CBUFFER_END

            struct Entrada { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Saida
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fog : TEXCOORD3;
            };

            Saida Vert(Entrada i)
            {
                Saida o;
                VertexPositionInputs p = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 Frag(Saida i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                clip(t.a - _Cutoff);
                float3 n = normalize(i.normalWS);
                Light luz = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float nl = saturate(dot(n, luz.direction));
                float sombra = lerp(0.55, 1.0, luz.shadowAttenuation);
                half3 amb = SampleSH(n);
                half3 c = t.rgb * (amb + luz.color * (0.25 + 0.75 * nl) * sombra);
                float3 v = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float3 h = normalize(luz.direction + v);
                c += luz.color * pow(saturate(dot(n, h)), 48.0) * _Brilho * sombra;
                c = lerp(c, _CorPiscar.rgb, _CorPiscar.a);
                c = MixFog(c, i.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
    }
}
