Shader "VSX.UniversalVehicleCombat/Hologram" {
	Properties {
  		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_RimPower ("Rim Power", Range(0.5,20)) = 3.0
		_Opacity ("Opacity", Range(0,1)) = 0.25
	}

	SubShader {
	  	Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }

	  	CGPROGRAM
	  	#pragma surface surf Unlit alpha
	  	struct Input {
	   	   	float2 uv_NormalMap;
	   	   	float3 viewDir;
	  	};

	  	sampler2D _NormalMap;

	  	float4 _RimColor;
	  	float _RimPower;
		float _Opacity;
	
    	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
	    {
	         return half4(s.Albedo, s.Alpha);
	    }

	  	void surf (Input IN, inout SurfaceOutput o) {
			o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
	      	half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Alpha = pow (rim, _RimPower) * _Opacity;
			o.Emission = _RimColor.rgb * pow (rim, _RimPower);
	  	}
	  	ENDCG
	} 
	Fallback "Diffuse"
}