Shader "Sprites/Custom/URP_SpriteShadowOutline"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        
        [Header(Outline Settings)]
        [HDR] _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Cull Off
        ZWrite On

        // ---------------------------------------------------------
        // PASS 1: UNIVERSAL FORWARD (Rendering Visual & Outline)
        // ---------------------------------------------------------
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize; 

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Cutoff;
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                float2 offset = _MainTex_TexelSize.xy * _OutlineWidth;
                half alphaUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, offset.y)).a;
                half alphaDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -offset.y)).a;
                half alphaLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-offset.x, 0)).a;
                half alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(offset.x, 0)).a;
                
                half outlineAlpha = saturate(alphaUp + alphaDown + alphaLeft + alphaRight);
                outlineAlpha = saturate(outlineAlpha - texColor.a);
                
                half combinedAlpha = saturate(texColor.a + outlineAlpha);
                clip(combinedAlpha - _Cutoff);
                
                half4 baseColor = texColor * input.color * _Color;
                half4 finalColor = lerp(baseColor, _OutlineColor, outlineAlpha);
                
                return finalColor;
            }
            ENDHLSL
        }

        // ---------------------------------------------------------
        // PASS 2: SHADOW CASTER (Mencetak Bayangan)
        // ---------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Cutoff;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                // Perbaikan utama: Menggunakan transformasi ruang standar (OS -> CS)
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                float2 offset = _MainTex_TexelSize.xy * _OutlineWidth;
                half alphaUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, offset.y)).a;
                half alphaDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -offset.y)).a;
                half alphaLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-offset.x, 0)).a;
                half alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(offset.x, 0)).a;
                
                half outlineAlpha = saturate(alphaUp + alphaDown + alphaLeft + alphaRight);
                outlineAlpha = saturate(outlineAlpha - texColor.a);
                
                half combinedAlpha = saturate(texColor.a + outlineAlpha);
                
                // Klip alpha di pass bayangan agar bentuk bayangan sesuai dengan outline
                clip(combinedAlpha - _Cutoff);
                
                return 0; 
            }
            ENDHLSL
        }
    }
    Fallback Off
}