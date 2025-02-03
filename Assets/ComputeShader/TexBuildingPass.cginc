




void FillPass(inout RWTexture3D<float4> outTex, float4 col, int3 uv)
{
    outTex[uv] = col;
}