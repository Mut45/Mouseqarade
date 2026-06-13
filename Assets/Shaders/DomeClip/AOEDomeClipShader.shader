Shader "Custom/AOE/Dome_Clip"
{
    Properties
    {
        _Color ("Color", Color) = (0.3, 0.8, 1.0, 0.25)
        _ClipZ ("Clip Z", Float) = 0 // This defines at which z value plane should the sphere be cut off
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            ZWrite Off // From Unity Documentation: ZWrite On | Off Controls whether pixels from this object are written to the depth buffer (default is On). If you're drawng solid objects, leave this on. If you're drawing semitransparent effects, switch to ZWrite Off. For more details read below.
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;
            float _ClipZ;

            struct vertexInput
            {
                float4 vertex : POSITION;
            };

            struct fragmentInput
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fragmentInput vert(vertexInput v)
            {
                fragmentInput o;

                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = world.xyz;
                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(fragmentInput i) : SV_Target
            {
                clip(_ClipZ - i.worldPos.z);

                return _Color;
            }

            ENDCG
        }
    }
}