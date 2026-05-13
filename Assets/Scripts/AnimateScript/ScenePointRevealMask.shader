Shader "UI/ScenePointRevealMask"
{
    Properties
    {
        _Color ("Mask Color", Color) = (0, 0, 0, 1)
        _Radius ("Radius", Float) = 0
        _Aspect ("Aspect", Float) = 1
        _EdgeSoftness ("Edge Softness", Float) = 0.0002
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
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

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _Radius;
            float _Aspect;
            float _EdgeSoftness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv - float2(0.5, 0.5);
                centered.x *= _Aspect;
                float dist = length(centered);
                float alpha = smoothstep(_Radius, _Radius + _EdgeSoftness, dist) * _Color.a;
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}
