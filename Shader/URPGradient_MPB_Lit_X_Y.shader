Shader "Custom/URPGradient_MPB_Lit_X_Y"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1, 0.5, 0.4, 1)
        _ColorTop    ("Top Color", Color)    = (0.2, 0.9, 0.8, 1)
        _Metallic    ("Metallic", Range(0, 1)) = 0.0
        _Smoothness  ("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Stencil
            {
                Ref 1        // Stencil Value
                Comp Always  // Always write
                Pass Replace // Write buffer with stencil
            }

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
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.worldPos);

                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = max(dot(normalWS, lightDir), 0.0);

                // Gradient albedo (X ve Y ayrý ayrý)
                float3 colorBottom = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorBottom).rgb;
                float3 colorTop    = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTop).rgb;

                float tx = saturate(IN.uv.x);
                float ty = saturate(IN.uv.y);

                float3 gradX = lerp(colorBottom, colorTop, tx);
                float3 gradY = lerp(colorBottom, colorTop, ty);
                float3 albedo = (gradX + gradY) * 0.5;

                // Lambert diffuse
                float3 diffuse = albedo * mainLight.color.rgb * NdotL;

                // Specular
                float3 halfVec = normalize(lightDir + viewDirWS);
                float NdotH = max(dot(normalWS, halfVec), 0.0);
                float smoothness = UNITY_ACCESS_INSTANCED_PROP(Props, _Smoothness);
                float3 specular = pow(NdotH, 32.0) * smoothness * mainLight.color.rgb;

                // Ambient
                float3 ambient = albedo * 0.2;

                float metallic = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
                float3 finalColor = ambient + diffuse + specular * metallic;

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
