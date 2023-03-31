// Adapted from https://github.com/Scrawk/Interactive-Erosion
// Consists mostly of code written by Scrawk, with some modifications by me

Shader "FluidSim/WaterShader" 
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		skyColor("Sky Color", Color) = (0,0,1,1)
		waterAbsoprtion("Water Absorption", Vector) = (0.259, 0.086, 0.113, 2000.0)
		fresnelFactor("Fresnel Factor", Float) = 4.0
		minWaterHeight("Minimum Water Height", Float) = 1.0
		sunSpectralStr("SunSpecStr", Float) = 0.4
	}
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		
		GrabPass { "_GrabTex" }
		
		CGPROGRAM
		#pragma exclude_renderers gles
		#pragma surface surf Lambert vertex:vert noforwardadd nolightmap
		#pragma target 3.0
		#pragma glsl

		sampler2D _MainTex, _GrabTex, _CameraDepthTexture;
		float4 skyColor, waterAbsoprtion;
		float fresnelFactor, minWaterHeight, sunSpectralStr;
		
		uniform sampler2D waterHeight, waterVelocity;
		uniform float resolution;
		uniform float3 sunDirection;
		
		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
			float4 grabUV;
			float4 projPos;
			float depth;
		};
		
		float GetTotalHeight(float4 texData) 
		{
			return texData.x + texData.y + texData.z + texData.w;
		}
		
		void vert(inout appdata_tan v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o)
			
			v.tangent = float4(1,0,0,1);
		
			v.vertex.y += (GetTotalHeight(tex2Dlod(_MainTex, float4(v.texcoord.xy, 0.0, 0.0)))) + (tex2Dlod(waterHeight, float4(v.texcoord.xy, 0.0, 0.0)).x);
			
			float4 pos = UnityObjectToClipPos (v.vertex);
			o.grabUV = ComputeGrabScreenPos(pos);
			o.projPos = ComputeScreenPos(pos);
			o.depth = pos.z / pos.w;
		}
		
		float3 FindNormal(float2 uv, float u)
        {
        	float ht0 = GetTotalHeight(tex2D(_MainTex, uv + float2(-u, 0)));
            float ht1 = GetTotalHeight(tex2D(_MainTex, uv + float2(u, 0)));
            float ht2 = GetTotalHeight(tex2D(_MainTex, uv + float2(0, -u)));
            float ht3 = GetTotalHeight(tex2D(_MainTex, uv + float2(0, u)));
      
            ht0 += tex2D(waterHeight, uv + float2(-u, 0)).x;
            ht1 += tex2D(waterHeight, uv + float2(u, 0)).x;
            ht2 += tex2D(waterHeight, uv + float2(0, -u)).x;
            ht3 += tex2D(waterHeight, uv + float2(0, u)).x;
            
            float2 _step = float2(1.0, 0.0);

            float3 va = normalize(float3(_step.xy, ht1-ht0));
            float3 vb = normalize(float3(_step.yx, ht2-ht3));

           return cross(va,vb);
        }
        
        float3 Sun(float3 V, float3 N)
		{
			float3 H = normalize(V+sunDirection.xzy);
			return pow(abs(dot(H,N)), 512).xxx;
		}

		void surf(Input IN, inout SurfaceOutput o) 
		{
			float ht = tex2D(waterHeight, IN.uv_MainTex).x;
	
			if (ht < minWaterHeight) discard;
		
			float3 N = FindNormal(IN.uv_MainTex, 1.0/resolution);
			
			float3 V = normalize(_WorldSpaceCameraPos-IN.worldPos).xzy;
			
			float fresnel = exp(-max(dot(V,N),0.0) * fresnelFactor);
			
			float3 grab = tex2Dproj(_GrabTex, UNITY_PROJ_COORD(IN.grabUV)).rgb;
			
			float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(IN.projPos)).r);
			float fragmentsDepth = Linear01Depth(IN.depth);
			
			float waterDepth = clamp(depth - fragmentsDepth, 0.0, 1.0);

			float3 AbsorptonCof = waterAbsoprtion.rgb * waterDepth * waterAbsoprtion.a;
			
			float3 col = grab * exp(-AbsorptonCof*AbsorptonCof);
			
			o.Albedo = lerp(col, skyColor.rgb, fresnel*0.4) + Sun(V,N) * sunSpectralStr;
			o.Alpha = 1.0;
			o.Normal = N;
			
			//o.Albedo = tex2D(_SedimentField, IN.uv_MainTex).xxx;
			
			o.Albedo = length(tex2D(waterVelocity, IN.uv_MainTex).xy).xxx*0.1;
			
			
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
