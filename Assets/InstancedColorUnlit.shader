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
            #include "UnityCG.cginc"

            StructuredBuffer<uint> _ColorIdBuffer;
            StructuredBuffer<float4> _ColorBuffer;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i, uint instanceID : SV_InstanceID) : SV_Target
            {
                uint colorIndex = _ColorIdBuffer[instanceID];
                float4 col = _ColorBuffer[colorIndex];
                col.a = 1.0;
                return col;
            }
            ENDCG
        }
    }
}
