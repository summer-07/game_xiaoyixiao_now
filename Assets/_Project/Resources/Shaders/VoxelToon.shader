// Lightweight lit shader for voxel cubes and wool balls.
// Lives under Resources so Shader.Find works in WebGL / mini-game builds without a material asset.
Shader "SheepStrike/VoxelToon"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Ambient ("Ambient", Range(0, 1)) = 0.55
        _Rim ("Rim", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            half _Ambient;
            half _Rim;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                half3 normal : TEXCOORD0;
                half3 viewDir : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                half3 n = normalize(i.normal);
                // Works without a light too: _WorldSpaceLightPos0 is zero, leaving ambient only.
                half diffuse = saturate(dot(n, normalize(_WorldSpaceLightPos0.xyz + half3(0, 0.0001, 0))));
                half top = saturate(n.y) * 0.15;
                half lit = _Ambient + (1 - _Ambient) * diffuse + top;
                half rim = pow(1 - saturate(dot(n, normalize(i.viewDir))), 3) * _Rim;
                return fixed4(color.rgb * lit + rim, 1);
            }
            ENDCG
        }
    }
    FallBack "VertexLit"
}
