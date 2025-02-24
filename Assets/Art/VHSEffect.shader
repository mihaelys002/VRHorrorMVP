Shader"Custom/VHSAlienShader"
{
    Properties
    {
        _MainTex ("Camera Render Texture", 2D) = "white" {}
        _TimeScale ("Time Scale", Float) = 1.0
        _NoiseAmount ("Noise Amount", Float) = 0.05
        _ScanLineIntensity ("Scan Line Intensity", Float) = 1.0
        _ColorShift ("Color Shift", Float) = 0.02
        _AlienGreen ("Alien Green Tint", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
LOD 100

        Pass
        {
            CGPROGRAM
            // Define vertex and fragment functions
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float _TimeScale;
float _NoiseAmount;
float _ScanLineIntensity;
float _ColorShift;
float _AlienGreen;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

            // Simple pseudo-random noise function
float rand(float2 co)
{
    return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
}

fixed4 frag(v2f i) : SV_Target
{
    float2 uv = i.uv;

                // --- VHS Distortion ---
                // Horizontal jitter based on vertical position and time
    float jitter = sin(uv.y * 50.0 + _Time.y * _TimeScale) * 0.005;
    uv.x += jitter;

                // --- Base Texture ---
    fixed4 color = tex2D(_MainTex, uv);

                // --- Chromatic Aberration ---
                // Slightly offset red and blue channels for a color misalignment effect
    float red = tex2D(_MainTex, uv + float2(_ColorShift, 0)).r;
    float blue = tex2D(_MainTex, uv - float2(_ColorShift, 0)).b;
    color.r = red;
    color.b = blue;

                // --- Scan Lines ---
                // High-frequency horizontal lines across the image
    float scanLine = sin(uv.y * 800.0) * _ScanLineIntensity * 0.05;
    color.rgb -= scanLine;

                // --- Noise/Static ---
                // Add subtle static to enhance the analog feel
    float noise = (rand(uv * _Time.y) - 0.5) * _NoiseAmount;
    color.rgb += noise;

                // --- Glitch Horizontal Line Effect ---
                // Occasionally overlay a bright horizontal glitch line across the screen
    float glitchChance = frac(sin(_Time.y * 25.0) * 43758.5453);
    float glitchTrigger = smoothstep(0.995, 1.0, glitchChance); // Rare trigger
    float glitchY = frac(sin(_Time.y * 55.0) * 43758.5453); // Random vertical position
    float glitchThickness = 0.005; // Line thickness
    float glitchLineAlpha = 1.0 - smoothstep(0.0, glitchThickness, abs(uv.y - glitchY));
    float glitchEffect = glitchTrigger * glitchLineAlpha;
                // Blend the current color with a white line effect
    color.rgb = lerp(color.rgb, float3(1.0, 1.0, 1.0), glitchEffect * 0.5);

                // --- Alien Look ---
                // Boost the green channel for an otherworldly tint
    color.g += _AlienGreen * 0.1;

    return color;
}
            ENDCG
        }
    }
FallBack"Diffuse"
}
