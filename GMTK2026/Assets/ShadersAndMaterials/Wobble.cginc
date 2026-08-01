#ifndef WOBBLE_INCLUDED
#define WOBBLE_INCLUDED

float _WobbleAmountPx;
float _WobbleFPS;
float _PixelsPerUnit;
float _WobbleFrequency;
float _WobbleProbability;
float _WobbleSnapToPixel;

float2 hash22(float2 p)
{
    float2 k = float2(12.9898, 78.233);
    float n1 = frac(sin(dot(p, k)) * 43758.5453);
    float n2 = frac(sin(dot(p + 17.13, k * 1.7)) * 24634.6345);
    return float2(n1, n2);
}

float hash21(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 ApplyWobble(float4 vertex, float2 uv)
{
    if (_WobbleAmountPx > 0.0001 && _WobbleProbability > 0.0001)
    {
        // Per-object seed wrapped to safe bounds
        float objSeed = frac(uv.x * 12.9898 + uv.y * 78.233) * 100.0;

        // Wrap step frame sequence using fmod so numbers stay small (prevents GPU sin() precision loss)
        float fps = max(_WobbleFPS, 1.0);
        float step = fmod(floor(_Time.y * fps), 3600.0);

        float2 noiseCoord = uv * _WobbleFrequency + objSeed + step * 1.618;

        // Gate: only a fraction of vertices move per frame
        float gate = hash21(noiseCoord + 99.0);
        if (gate < _WobbleProbability)
        {
            float2 n = hash22(noiseCoord);
            n = (n - 0.5) * 2.0; // -1..1

            float texel = 1.0 / max(_PixelsPerUnit, 0.0001);
            float2 rawOffsetTexels = n * _WobbleAmountPx;

            float2 offsetTexels;
            if (_WobbleSnapToPixel > 0.5)
            {
                offsetTexels = round(rawOffsetTexels);
            }
            else
            {
                offsetTexels = rawOffsetTexels;
            }

            vertex.xy += offsetTexels * texel;
        }
    }
    return vertex;
}

#endif