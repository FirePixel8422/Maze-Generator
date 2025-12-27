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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                uint instanceID : TEXCOORD0;
            };

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.instanceID = instanceID;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                uint colorIndex = 0;

                #ifdef USE_COLOR_ID_BUFFER
                colorIndex = _ColorIdBuffer[i.instanceID];
                #endif

                float4 col = _ColorBuffer[colorIndex];
                return col;
            }
            ENDCG
        }
    }
}
