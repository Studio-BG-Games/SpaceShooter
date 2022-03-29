
// VSXGames Energy Shield Effect Shader 
Shader "VSX/UniversalVehicleCombat/Energy Shield" 
{
	Properties
	{
		
		_MainTex("Texture (RGB)", 2D) = "white" {}
		
		[Header(Hit Effect)]

		_EffectShapeNoiseTex("Effect Shape Noise Texture (RGB)", 2D) = "white" {}
		_EffectShapeAmount("Effect Shape Amount", Range(0, 4)) = 0.5
		_GrowthSpeed("Effect Growth Speed", float) = 8

		_EffectEdgeStrength("Effect Edge Amount", float) = 1
		_EffectEdgeSharpness("Effect Edge Sharpness", float) = 10
		_EffectInnerGlowStrength("Effect Inner Glow Strength", float) = 0.03

		[Header(Rim Glow)]

		[HDR]_RimColor("Rim Color", Color) = (0.75, 2, 4)
		_RimOpacity("Rim Opacity", Range(0, 1)) = 1
		_RimEdgeAmount("Rim Edge Amount", Range(0.5,20)) = 5

		[Header(Effect Instances)]
		
		// Buffer of hit positions, can add more
		_EffectPosition0 ("Effect Position 0",Vector) = (0,0,0,0)
        _EffectPosition1 ("Effect Position 1",Vector) = (0,0,0,0)
		_EffectPosition2 ("Effect Position 2",Vector) = (0,0,0,0)
		_EffectPosition3 ("Effect Position 3",Vector) = (0,0,0,0)
		_EffectPosition4 ("Effect Position 4",Vector) = (0,0,0,0)
		_EffectPosition5 ("Effect Position 5",Vector) = (0,0,0,0)
		_EffectPosition6 ("Effect Position 6",Vector) = (0,0,0,0)
		_EffectPosition7 ("Effect Position 7",Vector) = (0,0,0,0)
		_EffectPosition8 ("Effect Position 8",Vector) = (0,0,0,0)
		_EffectPosition9 ("Effect Position 9",Vector) = (0,0,0,0)

		[Header(Effect Colors)]

		[HDR] _EffectColor0 ("Effect Color 0", Color) = (0,0,0,1)
		[HDR] _EffectColor1 ("Effect Color 1", Color) = (0,0,0,1)
		[HDR] _EffectColor2 ("Effect Color 2", Color) = (0,0,0,1)
		[HDR] _EffectColor3 ("Effect Color 3", Color) = (0,0,0,1)
		[HDR] _EffectColor4 ("Effect Color 4", Color) = (0,0,0,1)
		[HDR] _EffectColor5 ("Effect Color 5", Color) = (0,0,0,1)
		[HDR] _EffectColor6 ("Effect Color 6", Color) = (0,0,0,1)
		[HDR] _EffectColor7 ("Effect Color 7", Color) = (0,0,0,1)
		[HDR] _EffectColor8 ("Effect Color 8", Color) = (0,0,0,1)
		[HDR] _EffectColor9 ("Effect Color 9", Color) = (0,0,0,1)
 
    }

    SubShader 
	{

		ZWrite Off
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}
		Blend One One
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_fog_exp2

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : TEXCOORD1;
				float4 pos : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_EffectShapeNoiseTex : TEXCOORD3;
				float3 normal : TEXCOORD2;
			};


			// Declare the effect positions
			uniform float4 _EffectPosition0;
			uniform float4 _EffectPosition1;
			uniform float4 _EffectPosition2;
			uniform float4 _EffectPosition3;
			uniform float4 _EffectPosition4;
			uniform float4 _EffectPosition5;
			uniform float4 _EffectPosition6;
			uniform float4 _EffectPosition7;
			uniform float4 _EffectPosition8;
			uniform float4 _EffectPosition9;

			// Declare the effect colors
			uniform float4 _EffectColor0;
			uniform float4 _EffectColor1;
			uniform float4 _EffectColor2;
			uniform float4 _EffectColor3;
			uniform float4 _EffectColor4;
			uniform float4 _EffectColor5;
			uniform float4 _EffectColor6;
			uniform float4 _EffectColor7;
			uniform float4 _EffectColor8;
			uniform float4 _EffectColor9;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _EffectShapeNoiseTex;
			float4 _EffectShapeNoiseTex_ST;
			uniform float _EffectShapeAmount;

			uniform float _GrowthSpeed;

			uniform float _EffectEdgeStrength;
			uniform float _EffectEdgeSharpness;

			uniform float _EffectInnerGlowStrength;

			
			// Vertex function
			v2f vert(appdata_full v)
			{

				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.vertex = v.vertex;
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_EffectShapeNoiseTex = TRANSFORM_TEX(v.texcoord, _EffectShapeNoiseTex);
				o.normal = v.normal;
				
				return o;
			}

			// Fragment function
			half4 frag(v2f o) : COLOR
			{
				_EffectInnerGlowStrength = max(_EffectInnerGlowStrength, 0);

				// Calculate values used across all hit effects
				float noise = _EffectShapeAmount * tex2D(_EffectShapeNoiseTex, o.uv_EffectShapeNoiseTex);
				
				// Effect 0
				float size = _GrowthSpeed * _EffectPosition0.w;															// Calculate the size based on the time since start			
				float amount = size - distance(o.vertex, _EffectPosition0.xyz) - noise;									// Calculate an amount for the effect based on dist from hit point.
				float edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));			// Calculate the edge amount
				float inner = max(distance(o.vertex, _EffectPosition0.xyz), 1);											// Calculate the inner glow amount (fading out toward the center)
				inner = min(max(amount, 0) * 100, 1) * inner;															// Only show the inner glow if it's inside the hit effect area
				half4 effectColor0 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor0;	// Put everything together to get the color for this hit effect
				
				// Effect 1
				size = _GrowthSpeed * _EffectPosition1.w;															
				amount = size - distance(o.vertex, _EffectPosition1.xyz) - noise;									
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));			
				inner = max(distance(o.vertex, _EffectPosition1.xyz), 1);											
				inner = min(max(amount, 0) * 100, 1) * inner;															
				half4 effectColor1 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor1;	

				// Effect 2
				size = _GrowthSpeed * _EffectPosition2.w;
				amount = size - distance(o.vertex, _EffectPosition2.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition2.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor2 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor2;

				// Effect 3
				size = _GrowthSpeed * _EffectPosition3.w;
				amount = size - distance(o.vertex, _EffectPosition3.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition3.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor3 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor3;

				// Effect 4
				size = _GrowthSpeed * _EffectPosition4.w;
				amount = size - distance(o.vertex, _EffectPosition4.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition4.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor4 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor4;

				// Effect 5
				size = _GrowthSpeed * _EffectPosition5.w;
				amount = size - distance(o.vertex, _EffectPosition5.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition5.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor5 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor5;

				// Effect 6
				size = _GrowthSpeed * _EffectPosition6.w;
				amount = size - distance(o.vertex, _EffectPosition6.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition6.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor6 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor6;

				// Effect 7
				size = _GrowthSpeed * _EffectPosition7.w;
				amount = size - distance(o.vertex, _EffectPosition7.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition7.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor7 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor7;

				// Effect 8
				size = _GrowthSpeed * _EffectPosition8.w;
				amount = size - distance(o.vertex, _EffectPosition8.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition8.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor8 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor8;

				// Effect 9
				size = _GrowthSpeed * _EffectPosition9.w;
				amount = size - distance(o.vertex, _EffectPosition9.xyz) - noise;
				edge = (max((size - abs(amount) * (_EffectEdgeSharpness * size)), 0) / max(size, 0.001));
				inner = max(distance(o.vertex, _EffectPosition9.xyz), 1);
				inner = min(max(amount, 0) * 100, 1) * inner;
				half4 effectColor9 = (_EffectEdgeStrength * edge + _EffectInnerGlowStrength * inner) * _EffectColor9;

				float total = 0.0001;
				total += length(_EffectColor0);				
				total += length(_EffectColor1);
				total += length(_EffectColor2);
				total += length(_EffectColor3);
				total += length(_EffectColor4);
				total += length(_EffectColor5);
				total += length(_EffectColor6);
				total += length(_EffectColor7);
				total += length(_EffectColor8);
				total += length(_EffectColor9);

				half4 result = half4(0, 0, 0, 0);
				result += (length(_EffectColor0) / total) * effectColor0;
				result += (length(_EffectColor1) / total) * effectColor1;
				result += (length(_EffectColor2) / total) * effectColor2;
				result += (length(_EffectColor3) / total) * effectColor3;
				result += (length(_EffectColor4) / total) * effectColor4;
				result += (length(_EffectColor5) / total) * effectColor5;
				result += (length(_EffectColor6) / total) * effectColor6;
				result += (length(_EffectColor7) / total) * effectColor7;
				result += (length(_EffectColor8) / total) * effectColor8;
				result += (length(_EffectColor9) / total) * effectColor9;

				return result * tex2D(_MainTex, o.uv_MainTex);

			}

			ENDCG
		}

		Cull Back

		CGPROGRAM
		#pragma surface surf Unlit alpha

		struct Input 
		{
			float4 color : COLOR;
			float2 uv_NormalMap;
			float2 uv_MainTex;
			float3 viewDir;
		};

		sampler2D _MainTex;

		float4 _RimColor;
		float _RimOpacity;
		float _RimEdgeAmount;
	
		// Unlit lighting function
		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
		{
			return half4(s.Albedo, s.Alpha);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 tex = tex2D(_MainTex, IN.uv_MainTex);													// Get texture color
			half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));								// Get the rim amount
			o.Alpha = pow(rim, _RimEdgeAmount) * _RimOpacity;											// Apply settings to rim amount
			o.Albedo = pow(rim, _RimEdgeAmount) * _RimColor * tex2D(_MainTex, IN.uv_MainTex).rgb;		// Calculate albedo
		}

		ENDCG
	}

    Fallback "Transparent/VertexLit"
}
