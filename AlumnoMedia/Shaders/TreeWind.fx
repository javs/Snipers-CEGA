

float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float wind_wave;

static const float PI = 3.14159265f;

texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

struct VS_INPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VS_OUTPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float4 Color : COLOR0;
};

VS_OUTPUT_DEFAULT vs_simplewind(VS_INPUT_DEFAULT Input)
{
	VS_OUTPUT_DEFAULT Output;

	// Ajusta el viento a la parte superior del arbol
	//
	float y_adjust = Input.Position.y - 5.0f;

	if (y_adjust < 0.0f)
		y_adjust = 0.0f;

	Input.Position.x += wind_wave * 5.0f * (y_adjust / 60.0f);
	Input.Position.x += Input.Position.z * 0.2f * wind_wave;

	Output.Position = mul(Input.Position, matWorldViewProj);
	Output.Texcoord = Input.Texcoord;
	Output.Color = Input.Color;
	
	return Output;
}

float4 ps_noeffect(float2 Texcoord : TEXCOORD0) : COLOR0
{
	return tex2D(diffuseMap, Texcoord);
}

technique SimpleWind
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_simplewind();
		PixelShader  = compile ps_3_0 ps_noeffect();
	}
}
