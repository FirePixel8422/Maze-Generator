Shader "Custom/ProceduralMazeUnlit"
{
    Properties
    {
        _GridWidth ("Grid Width", Int) = 250
        _GridHeight ("Grid Height", Int) = 250
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            StructuredBuffer<float4> _ColorBuffer;
            StructuredBuffer<uint> _ColorIdBuffer;

            uint _GridWidth;
            uint _GridHeight;

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                uint instanceId : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                uint instanceId = v.instanceID;

                uint x = instanceId % _GridWidth;
                uint y = instanceId / _GridWidth;

                float2 gridSize = float2(_GridWidth, _GridHeight);

                float3 tileOffset = float3(
                    (float)x,
                    0,
                    (float)y
                );
                
                float3 centerOffset = float3(gridSize.x, 0, gridSize.y) * 0.5f;
                
                float3 worldPos = v.vertex.xyz + tileOffset - centerOffset;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                o.instanceId = instanceId;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                uint colorIndex = _ColorIdBuffer[i.instanceId];

                return _ColorBuffer[colorIndex];
            }

            ENDCG
        }
    }
}