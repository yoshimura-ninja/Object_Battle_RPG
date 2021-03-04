Shader "Hidden/FadeToBattle"
{
    Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Amount("Amount", Range(-0.1, 0.1)) = 0
		_Speed("Speed", float) = 50
	}
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
 
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
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
 
			sampler2D _MainTex;
			float _Amount;
			float _Speed;
 
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv + float2(sin(i.vertex.x * _Time[1] * _Speed) * _Amount, sin(i.vertex.y * _Time[0] * _Speed) * _Amount));
				return col;
			}
            ENDCG
        }
    }
}
