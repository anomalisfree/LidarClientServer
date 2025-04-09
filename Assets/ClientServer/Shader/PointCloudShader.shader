Shader "Custom/PointShader"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 1.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE;
            };

            uniform float _PointSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.pointSize = _PointSize;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 correctedColor = half4(pow(i.color.rgb, 2.2), i.color.a);
                return correctedColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
