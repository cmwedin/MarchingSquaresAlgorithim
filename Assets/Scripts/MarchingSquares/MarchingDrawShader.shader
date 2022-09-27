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
            float InverseLerp(float a, float b, float v) {
                float o = (v - a)/(b - a);
                return saturate(o);
            }

            float _CurveThickness;
            float _Threshold;
            float4 _CurveColor;
            float4 _AboveThresholdColor;
            float4 _BelowThresholdColor;

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
                float surfaceLevel : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            Interpolator vert (appdata v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.potentialValue = v.potentialValue.x;
                o.surfaceLevel = v.potentialValue.y;
                return o;
            }

            float4 frag (Interpolator i) : SV_Target
            {
                // sample the texture
                float4 col;
                // float t = abs(_Threshold - i.potentialValue)/_Threshold; 
                if(i.surfaceLevel == 0) {
                    col = _BelowThresholdColor;
                } else if (i.surfaceLevel == 1) {
                    col = _AboveThresholdColor;
                } else {
                    if(i.surfaceLevel > .5) {
                        float b = _CurveThickness * .5;
                        float t = InverseLerp(.5,.5+b,i.surfaceLevel);
                        col = lerp(_CurveColor,_AboveThresholdColor,t);
                    } else if(i.surfaceLevel < .5){
                        float a = _CurveThickness * .5;
                        float t = InverseLerp(.5-a,.5,i.surfaceLevel);
                        col = lerp(_BelowThresholdColor,_CurveColor,t);
                    } else {
                        col = _CurveColor;
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}
