Shader "VSX.UniversalVehicleCombat/EnterExitEffect" 
{
	Properties {
  		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_MainTex ("Texture", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_RimPower ("Rim Power", Range(0,20)) = 3.0
		_Opacity ("Opacity", Range(0,1)) = 0.25
		_FadePosition ("Fade Position", Range(0,1)) = 0.25
		_UVOffsetY("UV Offset Y", float) = 0
		_FadeStrength("Fade Strength", float) = 1
	}

	SubShader 
	{
	  	Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		
		
	  	CGPROGRAM
	  	#pragma surface surf Unlit alpha
		
	  	struct Input 
		{
	   	   	float2 uv_MainTex;
	   	   	float3 viewDir;
			float3 worldPos;
	  	};

	  	sampler2D _NormalMap;
		sampler2D _MainTex;

	  	float4 _RimColor;
	  	float _RimPower;
		float _Opacity;
		float _FadePosition;
		float _UVOffsetY;
		float _FadeStrength;
		
    	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
	    {
	         return half4(s.Albedo, s.Alpha);
	    }

	  	void surf (Input IN, inout SurfaceOutput o) 
		{
			float2 uvPos = float2(IN.uv_MainTex.x, IN.uv_MainTex.y + _UVOffsetY);
			o.Albedo = _RimColor * tex2D(_MainTex, uvPos);
			o.Normal = UnpackNormal (tex2D (_NormalMap, uvPos));
	      	half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
			
			// Fade toward the top
			float temp = 1 - (IN.uv_MainTex.y - _FadePosition) * _FadeStrength;
			temp = temp * temp * temp; 
			temp = min(temp, 1);
			temp = max(temp, 0);

			o.Alpha = pow (rim, _RimPower) * _Opacity * temp;
			
	  	}
	  	ENDCG
	} 
	Fallback "Diffuse"
}