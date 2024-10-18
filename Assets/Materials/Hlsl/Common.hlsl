#pragma once

float4 Lerp(float4 a, float4 b, float t)
{
    return a * (1 - t) + b * t;
}
// just like smoothstep(), but linear, not clamped
half invLerp(half from, half to, half value)
{
    return (value - from) / (to - from);
}
half invLerpClamp(half from, half to, half value)
{
    return saturate(invLerp(from, to, value));
}
// full control remap, but slower
half remap(half origFrom, half origTo, half targetFrom, half targetTo, half value)
{
    half rel = invLerp(origFrom, origTo, value);
    return lerp(targetFrom, targetTo, rel);
}
