Shader "Custom/OutlinePostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineTex ("Outline Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 0.8)
        _OutlineWidth ("Outline Width", Range(0.0, 10.0)) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off
        
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
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _OutlineTex;
            float4 _OutlineColor;
            float _OutlineWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);
                fixed outline = tex2D(_OutlineTex, i.uv).r;
                
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                
                float outlineValue = 0.0;
                int width = (int)_OutlineWidth;
                for (int x = -width; x <= width; x++)
                {
                    for (int y = -width; y <= width; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        float dist = length(offset);
                        if (dist <= _OutlineWidth)
                        {
                            float sample = tex2D(_OutlineTex, i.uv + offset).r;
                            outlineValue = max(outlineValue, sample);
                        }
                    }
                }
                
                float edge = outlineValue - outline;
                fixed4 col = original;
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, edge * _OutlineColor.a);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
