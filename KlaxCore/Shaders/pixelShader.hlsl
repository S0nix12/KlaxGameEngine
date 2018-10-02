
struct PSInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

float4 main(PSInput input) : SV_TARGET
{
	return input.color;
	//return float4(0.0f, 1.0f, 0.0f, 1.0f);
}