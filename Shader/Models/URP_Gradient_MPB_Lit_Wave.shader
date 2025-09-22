Shader "Custom/URP_Gradient_MPB_Lit_Wave"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1, 0.5, 0.4, 1) 
        _ColorTop    ("Top Color", Color)    = (0.2, 0.9, 0.8, 1)
        _Metallic    ("Metallic", Range(0, 1)) = 0.0
        _Smoothness  ("Smoothness", Range(0, 1)) = 0.5
        _StencilRef  ("Stencil Ref", Int) = 1 

        _EnableWave    ("Enable Wave", Float) = 1
        _WaveHeight    ("Wave Height", Float) = 0.027
        _Speed         ("Speed", Float) = 4
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
                Ref [_StencilRef]
                Comp Always
                Pass Replace
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

                UNITY_DEFINE_INSTANCED_PROP(float, _EnableWave)
                UNITY_DEFINE_INSTANCED_PROP(float, _WaveHeight)
                UNITY_DEFINE_INSTANCED_PROP(float, _Speed)
            UNITY_INSTANCING_BUFFER_END(Props)

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 pos = IN.positionOS;
                float time = _Time.y;

                float enableWave = UNITY_ACCESS_INSTANCED_PROP(Props, _EnableWave);
                float waveHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _WaveHeight);
                float speed      = UNITY_ACCESS_INSTANCED_PROP(Props, _Speed);

                if (enableWave > 0.5)
                {
                    float phase = frac(sin(dot(pos.xz, float2(12.9898,78.233))) * 43758.5453);
                    float wave = sin(pos.x * 1.5 + time * speed + phase * 6.28318)
                               + cos(pos.z * 1.5 + time * speed + phase * 6.28318);

                    pos.y += wave * waveHeight;
                }

                OUT.positionCS = TransformObjectToHClip(pos);
                OUT.worldPos = TransformObjectToWorld(pos);
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

                float3 colorBottom = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorBottom).rgb;
                float3 colorTop    = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorTop).rgb;

                float tx = saturate(IN.uv.x);
                float ty = saturate(IN.uv.y);

                float3 gradX = lerp(colorBottom, colorTop, tx);
                float3 gradY = lerp(colorBottom, colorTop, ty);
                float3 albedo = (gradX + gradY) * 0.5;

                float3 diffuse = albedo * mainLight.color.rgb * NdotL;

                float3 halfVec = normalize(lightDir + viewDirWS);
                float NdotH = max(dot(normalWS, halfVec), 0.0);
                float smoothness = UNITY_ACCESS_INSTANCED_PROP(Props, _Smoothness);
                float3 specular = pow(NdotH, 32.0) * smoothness * mainLight.color.rgb;

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
