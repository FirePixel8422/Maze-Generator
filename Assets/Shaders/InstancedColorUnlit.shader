Shader "Custom/ProceduralMazeUnlit"
{
    Properties
    {
        _TileSize ("Tile Size", Float) = 1
        _GridWidth ("Grid Width", Int) = 250
        _GridHeight ("Grid Height", Int) = 250
        _MazeOrigin ("Maze Origin", Vector) = (0,0,0,0)
        _UseColumnMajor ("Use Column Major", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 50

        Pass
        {
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            StructuredBuffer<float4> _ColorBuffer;
            StructuredBuffer<uint> _ColorIdBuffer;

            float _TileSize;
            uint _GridWidth;
            uint _GridHeight;
            float4 _MazeOrigin;
            int _UseColumnMajor;

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                nointerpolation uint instanceID : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                uint id = v.instanceID;

                uint x;
                uint y;

                // Row-major:
                // id = y * width + x
                //
                // Column-major:
                // id = x * height + y
                if (_UseColumnMajor == 1)
                {
                    x = id / _GridHeight;
                    y = id % _GridHeight;
                }
                else
                {
                    x = id % _GridWidth;
                    y = id / _GridWidth;
                }

                float3 tileOffset = float3(
                    (float)x * _TileSize - _GridWidth * 0.5,
                    0.0,
                    (float)y * _TileSize - _GridHeight * 0.5
                );

                float3 worldPos = v.vertex.xyz + tileOffset + _MazeOrigin.xyz;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.instanceID = id;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                uint maxIndex = (_GridWidth * _GridHeight);

                uint idx = min(i.instanceID, maxIndex - 1);

                uint colorIndex = _ColorIdBuffer[idx];

                return _ColorBuffer[colorIndex];
            }

            ENDCG
        }
    }
}