




void FillPass(RWTexture3D<float4> outTex, float4 col, int3 uv)
{
    outTex[uv] = col;
}