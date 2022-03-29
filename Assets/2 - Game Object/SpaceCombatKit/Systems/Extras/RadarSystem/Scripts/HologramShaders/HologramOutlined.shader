Shader "VSX.UniversalVehicleCombat/HologramOutlined"
{
	Properties
	{
		_OutlineColor ("Outline Color", Color) = (1,0.75,0.5,1)
		_OutlineWidth("Outline Width", Range(0.0,1.1)) = 0.01
		_Scale("Scale", float) = 1
		_RimColor ("Rim Color", Color) = (1,0.5,0,1)
		_NormalMap ("Normal Map", 2D) = "normal" {}
		_RimPower ("Rim Power", Range(0.2,5)) = 0.5
		_Opacity ("Opacity", Range(0,1)) = 0.75
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _NormalMap;
	half _OutlineWidth;
	half _Scale;
	float4 _HologramColor;
	float4 _OutlineColor;

	struct appdata {
		half4 vertex : POSITION;
		half4 uv : TEXCOORD0;
		half3 normal : NORMAL;
		fixed4 color : COLOR;
	};

	struct v2f {
		half4 pos : POSITION;
		half2 uv : TEXCOORD0;
		fixed4 color : COLOR;
	};
	ENDCG

	
	SubShader
	{
		Tags {
			"RenderType"="Opaque"
			"Queue" = "Transparent"
		}
		
		Pass {
			Name "BASE"
			Cull Back
			Blend Zero One
		}

		Pass{
			Name "OUTLINE"

			Cull Front
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				half3 norm = mul((half3x3)UNITY_MATRIX_IT_MV, v.normal);
				half2 offset = TransformViewToProjection(norm.xy);
				o.pos.xy += normalize(offset) * o.pos.z * _OutlineWidth * _Scale;
				o.color = _OutlineColor;
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
		
		CGPROGRAM
	  	#pragma surface surf Unlit alpha
	  	struct Input {
	   	   	float2 uv_NormalMap;
	   	   	float3 viewDir;
			float3 worldPos;
	  	};

	  	float4 _RimColor;
	  	float _RimPower;
		float _Opacity;
	
    	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
	    {
	         return half4(s.Albedo, s.Alpha);
	    }

	  	void surf (Input IN, inout SurfaceOutput o) {
			
			o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
	      	half rim = 1 - saturate(dot (normalize(IN.viewDir), o.Normal));
			o.Alpha = pow (rim, _RimPower) * _Opacity;
			o.Emission = _RimColor.rgb * pow (rim, _RimPower);
	  	}
	  	ENDCG
	}
	Fallback "Diffuse"
}
