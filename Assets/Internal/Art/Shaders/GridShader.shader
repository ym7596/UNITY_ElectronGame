Shader "Custom/GridShader"
{
    Properties
    {
        _GridColor ("Grid Color", Color) = (1, 0.92, 0, 0.4)
        _CellSize ("Cell Size", Float) = 2.0
        _LineWidth ("Line Width", Float) = 0.02
        _GridHalfWidth ("Grid Half Width", Float) = 30.0
        _GridHalfHeight ("Grid Half Height", Float) = 30.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            float4 _GridColor;
            float _CellSize;
            float _LineWidth;
            float _GridHalfWidth;
            float _GridHalfHeight;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float3 pos = input.positionWS;
                
                // 그리드 범위 내에 있는지 확인 (clipping)
                // GridManager의 중심 정렬 방식에 따라 abs(pos)로 체크
                if (abs(pos.x) > _GridHalfWidth || abs(pos.z) > _GridHalfHeight)
                {
                    discard;
                }

                // 월드 좌표 기반 그리드 라인 계산
                float2 dist = abs(frac(pos.xz / _CellSize + 0.5) - 0.5) * _CellSize;
                float val = min(dist.x, dist.y);
                
                // 선의 두께에 따라 알파 결정
                float lineAlpha = 1.0 - smoothstep(0.0, _LineWidth, val);
                
                if (lineAlpha <= 0.0) discard;
                
                return float4(_GridColor.rgb, _GridColor.a * lineAlpha);
            }
            ENDHLSL
        }
    }
}
