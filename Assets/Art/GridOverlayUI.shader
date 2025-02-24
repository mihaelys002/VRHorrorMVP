Shader"Custom/WorldGridOverlayWithText"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _LineStep ("Grid Cell Size", float) = 10.0
        _LineWidth ("Line Width", float) = 1.0
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _DigitAtlas ("Digit Atlas (r,0-9)", 2D) = "white" {}
        _TextSize ("Text Size (cell fraction)", float) = 0.3
        _TextColor ("Text Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
LOD 100
        
        Pass
        {
Cull Off

Lighting Off

ZWrite Off

Blend SrcAlpha
OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"
            
sampler2D _MainTex;
float4 _MainTex_ST;
float _LineStep;
float _LineWidth;
fixed4 _LineColor;
            
sampler2D _DigitAtlas;
float _TextSize;
fixed4 _TextColor;
            
struct appdata_t
{
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
    fixed4 color : COLOR;
};
            
struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    fixed4 color : COLOR;
};
            
v2f vert(appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                // Compute world position from the vertex.
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.color = v.color;
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
                // Sample the UI texture and modulate by vertex color.
    fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Only process non-transparent pixels.
    if (col.a <= 0.01)
        return col;
                
                // --- GRID DRAWING USING WORLD COORDINATES ---
                // Compute grid coordinates using world position.
    float2 gridUV = i.worldPos.xy / _LineStep;
                // Compute distance from the center of a grid cell.
    float2 gridDist = abs(frac(gridUV - 0.5) - 0.5);
    float2 aa = fwidth(gridUV);
                // Anti-aliased line intensity.
    float lineIntensity = 1.0 - smoothstep(0.0, _LineWidth * aa.x, min(gridDist.x, gridDist.y));
                // Blend the grid line color over the existing color.
    col.rgb = lerp(col.rgb, _LineColor.rgb, lineIntensity * _LineColor.a);
                
                // --- TEXT DRAWING ---
                // Determine the cell’s local coordinate in [0,1].
    float2 cellLocal = frac(i.worldPos.xy / _LineStep);
                // Define a horizontal text region centered in the cell.
                // We assume three characters in a row (a constant "r" and two digits).
    float totalTextWidth = 3.0 * _TextSize;
    float offsetX = (1.0 - totalTextWidth) * 0.5;
    float offsetY = 0.5 - _TextSize * 0.5;
                
                // Check if the current pixel falls within the text region.
    if (cellLocal.x >= offsetX && cellLocal.x <= (offsetX + totalTextWidth) &&
                   cellLocal.y >= offsetY && cellLocal.y <= (offsetY + _TextSize))
    {
                    // Compute which of the three character slots this pixel belongs to.
        float relativeX = cellLocal.x - offsetX;
        int slot = (int) floor(relativeX / _TextSize); // 0, 1, or 2
                    // Compute local coordinates within the character cell.
        float localX = frac(relativeX / _TextSize);
        float localY = (cellLocal.y - offsetY) / _TextSize;
                    
                    // Determine the atlas index for this slot.
        int atlasIndex;
        if (slot == 0)
        {
                        // Slot 0 is always the letter 'r'
            atlasIndex = 0;
        }
        else if (slot == 1)
        {
                        // Slot 1: tens digit based on the absolute world X cell index.
            int cellIndexInt = (int) abs(floor(i.worldPos.x / _LineStep));
            atlasIndex = (cellIndexInt / 10) + 1; // +1 because atlas index 0 is 'r'
        }
        else // slot == 2
        {
                        // Slot 2: ones digit.
            int cellIndexInt = (int) abs(floor(i.worldPos.x / _LineStep));
            atlasIndex = (cellIndexInt % 10) + 1;
        }
                    
                    // Convert atlasIndex to float and compute the atlas UV.
                    // We assume the digit atlas has 11 glyphs arranged horizontally.
        float atlasCharCount = 11.0;
        float2 atlasUV;
        atlasUV.x = (atlasIndex + localX) / atlasCharCount;
        atlasUV.y = localY; // assumes atlas is a single row
                    
                    // Sample the digit atlas.
        fixed4 digitSample = tex2D(_DigitAtlas, atlasUV);
                    // We assume the atlas uses its alpha channel as the glyph mask.
        float textMask = digitSample.a;
                    
                    // Composite the text color over the pixel.
                    // (You could also blend using the digitSample’s RGB if your atlas contains colored glyphs.)
        col.rgb = lerp(col.rgb, _TextColor.rgb, textMask * _TextColor.a);
    }
                
    return col;
}
            ENDCG
        }
    }
}
