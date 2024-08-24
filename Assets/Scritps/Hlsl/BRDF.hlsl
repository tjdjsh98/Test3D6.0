#ifndef _INCLUDE_SNEEPY_BRDF_HLSL
#define _INCLUDE_SNEEPY_BRDF_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

inline float4 LightingCookTorrance(SurfaceOutputCustom s, float3 viewDir, UnityGI gi)
{
    
    UnityLight light = gi.light;
    
    viewDir = normalize(viewDir);
    float3 lightDir = normalize(light.dir);
    s.Normal = normalize(s.Normal);
    
    float3 halfV = normalize(lightDir * viewDir);
    float NdotL = saturate(dot(s.Normal, lightDir));
    float NdotH = saturate(dot(s.Normal, halfV));
    float NdotV = saturate(dot(s.Normal, viewDir));
    float VdotH = saturate(dot(viewDir, halfV));
    float LdotH = saturate(dot(lightDir, halfV));
    
    // BRDFs
    
    float3 diff = DisnetDiff(s.Albedo, NdotL, NdotV, LdotH, _Roughness, _SpecColor);
    float3 spec = LightingCookTorrance(NdotL, LdotH, NdotV, _Roughtness, _SpecColor);
    
    
    float3 firstLayer = (diff * spec * _SpecColor) * _LightColor0.rgb;
    float4 c = float4(firstLayer, s.Alpha);
    
#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
    c.rgb += s.Albedo * gi.indirect.diffuse;
#endif
 
    return c;
}


float SchlickFresnel(float4 specColor, float3 lightDir, float3 halfVector)
{
    return specColor * (1 - specColor) * pow(1 - (dot(lightDir, halfVector)), 5);
}
float sqr(float value)
{
            
    return value * value;
        
}
 
float schlickFresnel(float value)
{
 
    float m = clamp(1 - value, 0, 1);
    return pow(m, 5);
            
}
 
        // CookTorrance Geometry Function
float G1(float k, float x)
{
    return x / (x * (1 - k) + k);
}

inline float3 CookTorranceSpec(float NdotL, float LdotH, float NdotH, float NdotV, float roughness, float F0)
{
    float alpha = sqr(roughness);
    float F, D, G;
 
            // D (Distribution Function)
 
    float alphaSqr = sqr(alpha);
    float denominator = sqr(NdotH) * (alphaSqr - 1.0) + 1.0f;
    D = alphaSqr / (PI * sqr(denominator));
 
            // F (Fresnel Function)
    float LdotH5 = ShlickFreenel(LdotH);
    F = F0 + (1.0 - F0) * LdotH5;
 
            // G (Geometry term, Schlick's Approximation of Smith
    float r = _Roughness + 1;
    float k = sqr(r) / 8;
    float glL = G1(k, NdotL);
    float glV = G1(k, NdotV);
    G = glL * glV;
 
    float specular = NdotL * D * F * G;
    return specular;
        
}
inline float3 DisneyDiff(float3 albedo, float NdotL, float NdotV, float LdotH, float roughness)
{
 
            // luminance approximation
    float albedoLuminosity = 0.3 * albedo.r
                + 0.6 * albedo.g
                + 0.1 * albedo.b;
            // normalize luminosity to isolate hue and saturation
    float3 albedoTint = albedoLuminosity > 0 ?
                                albedo / albedoLuminosity :
                                float3(1, 1, 1);
 
    float fresnelL = shlickFresnel(NdotL);
    float fresnelV = shlickFresnel(NdotV);
 
    float fresnelDiffuse = 0.5 + 2 * sqr(LdotH) * roughness;
 
    float diffuse = albedoTint
                            * lerp(1.0, fresnelDiffuse, fresnelL)
                            * lerp(1.0, fresnelDiffuse, fresnelV);
 
    float fresnelSubsurface90 = sqr(LdotH) * roughness;
 
    float fresnelSubsurface = lerp(1.0, fresnelSubsurface90, fresnelL)
                                    * lerp(1.0, fresnelSubsurface90, fresnelV);
 
    float ss = 1.25 * (fresnelSubsurface * (1 / (NdotL + NdotV) - 0.5) + 0.5);
 
    return saturate(lerp(diffuse, ss, _Subsurface) * (1 / PI) * albedo);
 
 
}

#endif