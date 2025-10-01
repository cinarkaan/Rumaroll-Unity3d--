Shader "Custom/URP_Gradient_Lit"
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

            ZWrite On // Write depth information 
            ZTest LEqual // if it closer , then fragment will be drawed

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
                half3  lighting   : TEXCOORD0; // hazýr aydýnlatma rengi
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

                // World normal & pos
                half3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                half3 worldPos = TransformObjectToWorld(IN.positionOS);
                half3 viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);

                // Gradient renk
                half2 uv = saturate(IN.uv.xy);

                half3 colorBottom = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorBottom).rgb;
                half3 colorTop    = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTop).rgb;

                half3 albedo = lerp(colorBottom, colorTop, (uv.x + uv.y) * 0.5);

                // Iþýk
                Light mainLight = GetMainLight();
                half3 lightDir = mainLight.direction;

                half NdotL = max(dot(normalWS, lightDir), 0.0);

                // Specular (vertex’te hesaplandý)
                half3 halfVec = normalize(lightDir + viewDirWS);
                half NdotH = max(dot(normalWS, halfVec), 0.0);
                half smoothness = UNITY_ACCESS_INSTANCED_PROP(Props, _Smoothness);
                half metallic   = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);

                half3 diffuse  = albedo * mainLight.color.rgb * NdotL;
                half3 specular = (NdotH * NdotH) * smoothness * mainLight.color.rgb * metallic;
                half3 ambient  = albedo * 0.2;

                OUT.lighting = ambient + diffuse + specular;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                return half4(IN.lighting, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
