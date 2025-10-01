Shader "Custom/URP_Gradient_Simple_Lit_Texture"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1, 0.5, 0.4, 1)
        _ColorTop ("Top Color", Color) = (0.2, 0.9, 0.8, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On // Write depth information 
            ZTest LEqual // if it closer , then fragment will be drawed


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS    : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorBottom)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorTop)
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex); // Tiling & offset uygulanýr
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // Normalize edilmeden alýndý, performans için küçük sapma kabul edilebilir
                half3 normalWS = IN.normalWS;

                Light mainLight = GetMainLight();
                half3 lightDir = mainLight.direction; // normalize edilmedi

                half t = saturate(IN.uv.y);

                half3 colorBottom = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorBottom).rgb;
                half3 colorTop = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTop).rgb;

                half3 gradientColor = lerp(colorBottom, colorTop, t);
                half3 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;

                half3 albedo = gradientColor * texColor;

                half NdotL = max(dot(normalWS, lightDir), 0.0);

                // Ambient + Diffuse ýþýklandýrma (specular yok)
                half3 finalColor = albedo * (0.2 + mainLight.color * NdotL);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
