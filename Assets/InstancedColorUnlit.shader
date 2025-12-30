Shader "Custom/InstancedColorUnlit"
{
    Properties { }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 50

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ USE_COLOR_ID_BUFFER

            #include "UnityCG.cginc"

            StructuredBuffer<float4> _ColorBuffer;

            #ifdef USE_COLOR_ID_BUFFER
            StructuredBuffer<uint> _ColorIdBuffer;
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                uint colorIndex = 0;

                #ifdef USE_COLOR_ID_BUFFER
                colorIndex = _ColorIdBuffer[unity_InstanceID];
                #endif

                float4 col = _ColorBuffer[colorIndex];
                return col;
            }
            ENDCG
        }
    }
}
