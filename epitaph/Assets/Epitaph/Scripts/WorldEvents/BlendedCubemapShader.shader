Shader "Skybox/BlendedCubemap"
{
    Properties
    {
        _Cubemap1("Cubemap 1 (Day)", CUBE) = "" {}
        _Cubemap2("Cubemap 2 (Night)", CUBE) = "" {}
        _Blend("Blend Factor", Range(0,1)) = 0
        _Rotation("Rotation", Range(0,360)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _Cubemap1;
            samplerCUBE _Cubemap2;
            float _Blend;
            float _Rotation;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Skybox'ta yön vektörü hesaplanır
                o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Cubemap yönünü döndür (Y ekseni etrafında)
                float3 dir = i.texcoord;
                float rad = radians(_Rotation);
                float cosR = cos(rad);
                float sinR = sin(rad);

                float3 rotatedDir;
                rotatedDir.x = dir.x * cosR - dir.z * sinR;
                rotatedDir.y = dir.y;
                rotatedDir.z = dir.x * sinR + dir.z * cosR;

                fixed4 col1 = texCUBE(_Cubemap1, rotatedDir);
                fixed4 col2 = texCUBE(_Cubemap2, rotatedDir);

                return lerp(col2, col1, _Blend);
            }
            ENDCG
        }
    }
    FallBack "Skybox/Cubemap"
}