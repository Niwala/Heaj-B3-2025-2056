static const int GAUSS_SAMPLES = 3;

static const float Weights[GAUSS_SAMPLES] =
{
	0.2270270270,
    0.3162162162,
    0.0702702703
};

static const float Offsets[GAUSS_SAMPLES] =
{
	0.0,
    1.3846153846,
    3.2307692308
};

void GaussianBlur_float(UnityTexture2D tex, UnitySamplerState samp, float2 uv, float2 texelSize, float2 direction, out float4 color)
{
	color = 0;

    [unroll]
	for (int i = 0; i < GAUSS_SAMPLES; i++)
	{
		float2 offset = Offsets[i] * direction * texelSize;
		float w = Weights[i];
		color += tex.Sample(samp, uv - offset) * w;
		if (i > 0)
			color += tex.Sample(samp, uv + offset) * w;
	}
}