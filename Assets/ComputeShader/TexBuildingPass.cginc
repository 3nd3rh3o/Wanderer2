




void FillPass(inout RWTexture2D<float4> outTex, float4 col, int2 uv)
{
    outTex[uv] = col;
}