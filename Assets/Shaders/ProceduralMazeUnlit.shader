Shader "Custom/ProceduralMazeUnlit"
{
    Properties
    {
        _GridWidth ("Grid Width", Int) = 250
        _GridHeight ("Grid Height", Int) = 250
        _TileSize ("Tile Size", Float) = 1
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
            float _TileSize;

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

                uint id = v.instanceID;

                uint x = id % _GridWidth;
                uint y = id / _GridWidth;

                float3 tileOffset =
                    float3(x, 0, y) * _TileSize
                    - float3(_GridWidth, 0, _GridHeight) * 0.5f * _TileSize;

                float3 worldPos = v.vertex.xyz + tileOffset;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                o.instanceId = id;

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