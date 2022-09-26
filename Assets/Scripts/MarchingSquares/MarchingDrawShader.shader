Shader "Unlit/MarchingDrawShader"
{
    Properties
    {
        _CurveThickness("Thickness",float) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 potentialValue : TEXCOORD1;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float potentialValue : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            Interpolator vert (appdata v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.potentialValue = v.potentialValue.x;
                return o;
            }

            float4 frag (Interpolator i) : SV_Target
            {
                // sample the texture
                float4 col;
                col.xyz = i.potentialValue;
                return col;
                // return float4(1,0,0,1);
            }
            ENDCG
        }
    }
}
