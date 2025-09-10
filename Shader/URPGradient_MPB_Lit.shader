Shader "Custom/URPGradient_MPB_Lit"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1, 0.5, 0.4, 1)
        _ColorTop ("Top Color", Color) = (0.2, 0.9, 0.8, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

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
                half3 worldPos    : TEXCOORD0;
                half3 normalWS    : TEXCOORD1;
                half2 uv          : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorBottom)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorTop)
                UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);

                // Normal vertex shader’da normalize edildi (half precision)
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));

                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.worldPos);

                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = max(dot(normalWS, lightDir), 0.0);

                // Gradient albedo
                half3 colorBottom = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorBottom).rgb;
                half3 colorTop = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTop).rgb;
                half t = saturate(IN.uv.y);
                half3 albedo = lerp(colorBottom, colorTop, t);

                // Lambert diffuse (ışık rengi + intensity dahil)
                float3 diffuse = albedo * mainLight.color.rgb * NdotL;

                // Specular
                float3 halfVec = normalize(float3(lightDir + viewDirWS));
                float NdotH = max(dot(float3(normalWS), halfVec), 0.0);
                half smoothness = UNITY_ACCESS_INSTANCED_PROP(Props, _Smoothness);
                float3 specular = pow(NdotH, 32.0) * smoothness * mainLight.color.rgb;

                // Ambient
                float3 ambient = albedo * 0.2;

                half metallic = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
                float3 finalColor = float3(ambient) + diffuse + specular * metallic;

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
