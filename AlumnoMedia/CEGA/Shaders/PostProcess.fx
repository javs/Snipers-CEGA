//
// Post-processing effects
//

struct VS_INPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

struct VS_OUTPUT_DEFAULT
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
};

float lens_radius = 80.0f;
float chromatic_aberrance_factor = 1.0f;
float blue_tint_factor = 1.0f;
float k = 2.0f;
float kcube = 2.0f;

float screen_w = 100.0f;
float screen_h = 100.0f;

float screen_center_x = 50.0f;
float screen_center_y = 50.0f;

VS_OUTPUT_DEFAULT vs_postprocess(VS_INPUT_DEFAULT Input)
{
	VS_OUTPUT_DEFAULT Output;

	// proyecta un plano
	Output.Position = float4(Input.Position.xy, 0, 1);
	Output.Texcoord = Input.Texcoord;

	return Output;
}

texture pre_render;
sampler pre_render_sampler = sampler_state
{
	Texture = (pre_render);
	MipFilter = NONE;
	MinFilter = NONE;
	MagFilter = NONE;
};

float4 ps_noeffect(float2 Texcoord : TEXCOORD0) : COLOR0
{
	return tex2D(pre_render_sampler, Texcoord);
}

/*
	Cubic Lens Distortion HLSL Shader

	Original Lens Distortion Algorithm from SSontech (Syntheyes)
	http://www.ssontech.com/content/lensalg.htm

	r2 = image_aspect*image_aspect*u*u + v*v
	f = 1 + r2*(k + kcube*sqrt(r2))
	u' = f*u
	v' = f*v

	author : Fran�ois Tarlier
	website : www.francois-tarlier.com/blog/index.php/2009/11/cubic-lens-distortion-shader

	JJ: modificaciones para agregar chromatic aberration
*/
float4 ps_lens_distortion(float2 tex : TEXCOORD0) : COLOR0
{
	float4 finalColor;

	float2 pos = float2(tex.x * screen_w, tex.y * screen_h);

	// jj: marco negro
	if (distance(pos, float2(screen_center_x, screen_center_y)) > lens_radius)
	{
		finalColor = float4(0.0f, 0.0f, 0.0f, 1.0f);
	}
	else
	{
		float r2 = (tex.x - 0.5) * (tex.x - 0.5) + (tex.y - 0.5) * (tex.y - 0.5);
		float f = 0;


		//only compute the cubic distortion if necessary 
		if (kcube == 0.0){
			f = 1 + r2 * k;
		}
		else{
			f = 1 + r2 * (k + kcube * sqrt(r2));
		};

		// get the right pixel for the current position
		float x = f*(tex.x - 0.5) + 0.5;
		float y = f*(tex.y - 0.5) + 0.5;

		// jj: chromatic aberration. crea minimas variaciones en cada canal de color,
		// que son distintas entre si.
		float3 inputDistordR = tex2D(pre_render_sampler, float2(x, y) + (0.0004f * chromatic_aberrance_factor));
		float3 inputDistordG = tex2D(pre_render_sampler, float2(x, y) - (0.0004f * chromatic_aberrance_factor));
		float3 inputDistordB = tex2D(pre_render_sampler, float2(x, y - (0.005f * chromatic_aberrance_factor)));

		// jj: tinte azul
		finalColor = float4(inputDistordR.r, inputDistordG.g, inputDistordB.b + (blue_tint_factor * 0.02f), 1);
	}

	return finalColor;
}


technique NoEffect
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_postprocess();
		PixelShader = compile ps_3_0 ps_noeffect();
	}
}

technique LensDistortion
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_postprocess();
		PixelShader  = compile ps_3_0 ps_lens_distortion();
	}
}

