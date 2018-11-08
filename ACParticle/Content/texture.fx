float4x4 xWorld;
float4x4 xView;
float4x4 xProjection;
Texture2D xTexture;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSizeX;
float xPointSpriteSizeY;
float xOpacity;

SamplerState TextureSampler
{
    Texture = <xTexture>;
    MinFilter = Anisotropic;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
    MaxAnisotropy = 16;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	//float3 Normal : TEXCOORD0;
    float4 Color : COLOR0;
    float2 TextureCoord : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

//------- Technique: ColoredNoShading --------

VertexShaderOutput ColoredNoShadingVS(float4 inPos : SV_POSITION, float4 inColor : COLOR)
{
    VertexShaderOutput Output = (VertexShaderOutput)0;
    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.Color = inColor;
    
    return Output;
}

PixelShaderOutput ColoredNoShadingPS(VertexShaderOutput PSIn)
{
    PixelShaderOutput Output = (PixelShaderOutput)0;
    
    Output.Color = PSIn.Color;

    return Output;
}

technique ColoredNoShading
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 ColoredNoShadingVS();
        PixelShader = compile ps_4_0 ColoredNoShadingPS();
    }
}

//------- Technique: TexturedNoShading --------

VertexShaderOutput TexturedNoShadingVS(float4 iPos : SV_POSITION, float2 iTexCoord : TEXCOORD0)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPos = mul(iPos, xWorld);
    float4 viewPos = mul(worldPos, xView);
    output.Position = mul(viewPos, xProjection);

	//output.Normal = input.Normal;
    output.TextureCoord = iTexCoord;

    return output;
}

PixelShaderOutput TexturedNoShadingPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = xTexture.Sample(TextureSampler, input.TextureCoord);

 	// only output completely opaque pixels
    output.Color.a *= xOpacity;
    clip(output.Color.a < 1.0f ? -1 : 1);

    return output;
}

PixelShaderOutput TexturedNoShadingTransPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;

    output.Color = xTexture.Sample(TextureSampler, input.TextureCoord);

	// only output semi-transparent pixels
    output.Color.a *= xOpacity;
    clip(output.Color.a < 1.0f && output.Color.a >= 0.03125f ? 1 : -1);

    return output;
}

technique TexturedNoShading
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 TexturedNoShadingVS();
        PixelShader = compile ps_4_0 TexturedNoShadingPS();
    }
    pass Pass1
    {
        ZWriteEnable = false;

        /*AlphaBlendEnable = true;
        DestBlend = InvSrcAlpha;
        SrcBlend = SrcAlpha;*/

        VertexShader = compile vs_4_0 TexturedNoShadingVS();
        PixelShader = compile ps_4_0 TexturedNoShadingTransPS();
    }
}

//------- Technique: PointSprite --------

VertexShaderOutput PointSpriteVS(float4 iPos : SV_POSITION, float2 iTexCoord : TEXCOORD0)
{
    VertexShaderOutput Output = (VertexShaderOutput)0;

    float3 center = mul(iPos, xWorld);
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector, xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector, eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (iTexCoord.x - 0.5f) * sideVector * 0.5f * xPointSpriteSizeX;
    finalPosition += (0.5f - iTexCoord.y) * upVector * 0.5f * xPointSpriteSizeY;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul(xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);

    Output.TextureCoord = iTexCoord;

    return Output;
}

PixelShaderOutput PointSpritePS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    output.Color = xTexture.Sample(TextureSampler, input.TextureCoord);

    // only output completely opaque pixels
    output.Color.a *= xOpacity;
    clip(output.Color.a < 1.0f ? -1 : 1);

    return output;
}

PixelShaderOutput PointSpriteTransPS(VertexShaderOutput input)
{
    PixelShaderOutput output = (PixelShaderOutput)0;
    output.Color = xTexture.Sample(TextureSampler, input.TextureCoord);

    // only output semi-transparent pixels
    output.Color.a *= xOpacity;
    clip(output.Color.a < 1.0f && output.Color.a >= 0.03125f ? 1 : -1);
    //clip(output.Color.a < 1.0f ? 1 : -1);

    return output;
}

technique PointSprite
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 PointSpriteVS();
        PixelShader = compile ps_4_0 PointSpritePS();
    }
    pass Pass1
    {
        ZWriteEnable = false;

        VertexShader = compile vs_4_0 PointSpriteVS();
        PixelShader = compile ps_4_0 PointSpriteTransPS();
    }
}