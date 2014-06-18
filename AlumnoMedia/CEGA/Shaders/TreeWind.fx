

float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float wind_wave;
float4 cameraPosition;

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
};

struct VS_OUTPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float Fog : FOG;
};

struct PS_OUTPUT
{
	float4 Color : COLOR0;
	//float Depth : DEPTH;
};

VS_OUTPUT_DEFAULT vs_simplewindtree(VS_INPUT_DEFAULT Input)
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

	// Calculo del factor de fog
	float4 cameraPosition = mul(Input.Position, matWorldView);
	Output.Fog = saturate((2500.0f - cameraPosition.z) / (2500.0f - 250.f));
	
	return Output;
}

VS_OUTPUT_DEFAULT vs_simplewindgrass(VS_INPUT_DEFAULT Input)
{
	VS_OUTPUT_DEFAULT Output;
	// Ajusta el viento a la parte superior del arbol
	//
	float y_adjust = Input.Position.y;

	//if (y_adjust < 0.0f)
	//	y_adjust = 0.0f;


	// Animate the upper vertices and normals only

	if (Input.Texcoord.y <= 0.1) {  // Or: if(v.TexCoords.y >= 0.9)

		Input.Position.z += wind_wave * 4.1f * (y_adjust / 60.0f);
		Input.Position.z += Input.Position.z * 0.1f * wind_wave;
		// Insert the code for 7.4.2, 7.4.3, or 7.4.4

	}

	Output.Position = mul(Input.Position, matWorldViewProj);
	Output.Texcoord = Input.Texcoord;

	// Calculo del factor de fog
	float4 cameraPosition = mul(Input.Position, matWorldView);
		Output.Fog = saturate((2500.0f - cameraPosition.z) / (2500.0f - 250.f));

	return Output;
}


PS_OUTPUT ps_fog(float2 Texcoord : TEXCOORD0, float Fog : FOG)
{
	PS_OUTPUT Output;

	float4 textureColor;
	float4 fogColor;

	textureColor = tex2D(diffuseMap, Texcoord);

	// fog lineal
	// JJ: por alguna razon, el device driver no aplica pixel fog a estos vertices (deberia?)
	// calcularlo manualmente.
	fogColor = float4(0.5f, 0.5f, 0.5f, 1.0f);
	Output.Color = Fog * textureColor + (1.0 - Fog) * fogColor;

	// correccion del alpha channel
	Output.Color.a = textureColor.a;

	return Output;
}

technique SimpleWindTree
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_simplewindtree();
		PixelShader = compile ps_3_0 ps_fog();
	}
}

technique SimpleWindGrass
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_simplewindgrass();
		PixelShader = compile ps_3_0 ps_fog();
	}
}
