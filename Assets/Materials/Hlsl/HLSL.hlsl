#pragma once

half invLerp(half from, half to, half value)
{
    return (value - from) / (to - from);
}