
struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
    float2 uv : TEXCOORD0;
};

sampler sampler0;
Texture2D texture0;

float4 Pixel(PS_INPUT input) : SV_Target
{
    float4 color = input.col * texture0.Sample(sampler0, input.uv);
    return color;
    //return float4(1, 1, 1, 1);
}