Shader "UI/SmokeDarknessCircle"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 1)
        _CircleCenter ("Circle Center", Vector) = (0.5, 0.5, 0, 0)
        _CircleRadius ("Circle Radius", Float) = 0.2
        _Softness ("Softness", Float) = 0.03
        _Alpha ("Alpha", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;

            float4 _CircleCenter;
            float _CircleRadius;
            float _Softness;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 spriteColor = tex2D(_MainTex, i.uv) * i.color;

                float2 delta = i.uv - _CircleCenter.xy;

                delta.x *= _ScreenParams.x / _ScreenParams.y;

                float dist = length(delta);
                
                float outsideCircle = smoothstep(
                    _CircleRadius - _Softness,
                    _CircleRadius + _Softness,
                    dist
                );

                fixed alpha = outsideCircle * _Alpha * _Color.a * spriteColor.a;

                return fixed4(_Color.rgb, alpha);
            }

            ENDCG
        }
    }
}