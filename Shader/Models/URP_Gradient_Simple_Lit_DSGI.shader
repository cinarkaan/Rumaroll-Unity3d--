Shader "Custom/URP_Gradient_Simple_Lit_DSGI"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1, 0.5, 0.4, 1)
        _ColorTop ("Top Color", Color) = (0.2, 0.9, 0.8, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        Cull Off   // Both side

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
                half4 color       : TEXCOORD0;
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

                // Transform
                half3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // Gradient hesaplama
                half t = saturate(IN.uv.y);
                half3 colorBottom = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorBottom).rgb;
                half3 colorTop    = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTop).rgb;
                half3 albedo      = lerp(colorBottom, colorTop, t);

                // Main light (normalize edilmiþ sabit yön kabul edildi)
                Light mainLight = GetMainLight();
                half3 lightDir = mainLight.direction; // normalize edilmiþ kabul ediliyor
                half NdotL = max(dot(normalWS, lightDir), 0.0);

                // GI contribution
                half3 gi = SampleSH(normalWS);

                // Ambient + Diffuse birleþik
                half3 finalColor = albedo * (gi + mainLight.color * NdotL);

                OUT.color = half4(finalColor, 1.0);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                return IN.color; // Pixel shader neredeyse boþ, tamamen vertex'te hesaplandý
            }
            ENDHLSL
        }
    }

    FallBack Off
}
