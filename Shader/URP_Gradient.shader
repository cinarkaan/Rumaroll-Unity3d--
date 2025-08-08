Shader "Custom/URP_Gradient"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1,0.5,0.4,1)
        _ColorTop ("Top Color", Color) = (0.2,0.9,0.8,1)
        _SpecParams ("Metallic (X) / Smoothness (Y)", Vector) = (0.0, 0.5, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float3 worldPos   : TEXCOORD0;
                half3  normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorBottom;
                float4 _ColorTop;
                float2 _SpecParams; // x: Metallic, y: Smoothness
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos   = TransformObjectToWorld(IN.positionOS);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half3 normalWS  = normalize(IN.normalWS);
                half3 viewDirWS = normalize(_WorldSpaceCameraPos.xyz - IN.worldPos);
                Light mainLight = GetMainLight();
                half3 lightDir  = normalize(mainLight.direction);

                half NdotL = max(dot(normalWS, lightDir), 0.0);
                half t     = saturate(IN.uv.y);
                half3 albedo = lerp(_ColorBottom.rgb, _ColorTop.rgb, t);

                half3 diffuse = albedo * mainLight.color * NdotL;

                // Basitleştirilmiş specular hesaplama (pow yerine)
                half3 halfDir = normalize(lightDir + viewDirWS);
                half NdotH    = saturate(dot(normalWS, halfDir));
                half3 specular = NdotH * _SpecParams.y;

                half3 ambient = albedo * 0.2;

                half3 finalColor = ambient + diffuse + specular * _SpecParams.x;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
