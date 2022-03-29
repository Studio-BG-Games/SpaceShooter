Shader "VSX/UniversalVehicleCombat/FillShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FillColor ("FillColor", Color) = (1,1,1,1)
		_BackgroundColor ("Background Color", Color) = (1,1,1,0.5)
		_FillAmount ("Fill Amount", float) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float _FillAmount;
			fixed4 _FillColor;
			fixed4 _BackgroundColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				float4 appliedColor;
				float multiplier = sign(_FillAmount - i.uv.y);
				multiplier = saturate(multiplier);
				appliedColor = multiplier * _FillColor + (1 - multiplier) * _BackgroundColor;
				
				col *= appliedColor;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
