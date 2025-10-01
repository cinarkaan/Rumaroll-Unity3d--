Shader "Custom/Water"
{
    Properties
    {
        _WaterTex("Water Texture", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NoiseTex("Noise Texture", 2D) = "gray" {}
        _Color("Water Color", Color) = (0,1,1,0.5)

        _WaveStrength("Wave Strength", Range(0,1)) = 0.1
        _WaveSpeed("Wave Speed", Range(0,3)) = 0.5
        _WaveHeight("Wave Height", Range(0,0.5)) = 0.05
        _DistortStrength("UV Distort Strength", Range(0,0.3)) = 0.03
        _ScaleStrength("UV Scale Strength", Range(0,0.1)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _WaterTex;
            sampler2D _NormalMap;
            sampler2D _NoiseTex;

            float4 _WaterTex_ST;
            float4 _NormalMap_ST;
            float4 _NoiseTex_ST;

            float4 _Color;
            float _WaveStrength;
            float _WaveSpeed;
            float _WaveHeight;
            float _DistortStrength;
            float _ScaleStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionH : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 posWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 pos = IN.positionOS.xyz;

                // Vertex dalga
                float phase = frac(sin(dot(pos.xz, float2(12.9898,78.233))) * 43758.5453);
                float waveX = sin(pos.x * 1.5 + _Time.y * _WaveSpeed + phase * 6.28318);
                float waveZ = cos(pos.z * 1.2 + _Time.y * _WaveSpeed * 0.8 + phase * 6.28318 * 0.7);
                pos.y += (waveX + waveZ) * 0.5 * _WaveHeight;

                OUT.positionH = TransformObjectToHClip(float4(pos,1.0));
                OUT.posWS = TransformObjectToWorld(float4(pos,1.0)).xyz;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Noise Distortion (sabit hızda rastgele yönlü)
                float2 nUV = TRANSFORM_TEX(IN.uv, _NoiseTex) * 2.0;
                float2 noise = tex2D(_NoiseTex, nUV + float2(_Time.y * _WaveSpeed * 0.3, _Time.y * _WaveSpeed * 0.2)).rg - 0.5;

                // --- UV distortion ve scale
                float2 uv = TRANSFORM_TEX(IN.uv, _WaterTex);
                uv += noise * _DistortStrength;
                uv *= (1.0 + noise.r * _ScaleStrength);

                // --- Double scroll ile tiling gizle
                float2 uv1 = uv + float2(_Time.y * 0.02, _Time.y * 0.015);
                float2 uv2 = uv + float2(-_Time.y * 0.01, _Time.y * 0.025);
                half4 col1 = tex2D(_WaterTex, uv1);
                half4 col2 = tex2D(_WaterTex, uv2);
                half4 baseCol = lerp(col1, col2, 0.5) * _Color;

                // --- Normal map distortion
                float2 normalUV = TRANSFORM_TEX(IN.uv, _NormalMap);
                half3 normalSample = UnpackNormal(tex2D(_NormalMap, normalUV));
                normalSample = normalize(normalSample * _WaveStrength + IN.normalWS);

                // --- Final color (diffuse only, Fresnel/specular opsiyonel)
                float NdotL = saturate(dot(normalSample, normalize(float3(0.3,1,0.3))));
                float3 finalColor = baseCol.rgb * NdotL;

                return half4(finalColor, baseCol.a);
            }

            ENDHLSL
        }
    }
}
