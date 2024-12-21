

PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID, UNITY_NEAR_CLIP_VALUE);
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.texCoord0 = output.positionCS * 0.5 + 0.5;

    output.texCoord0.y = 1 - output.texCoord0.y;
    
    float3 p = ComputeWorldSpacePosition(output.positionCS, UNITY_MATRIX_I_VP);

    // Encode view direction in texCoord1
    output.texCoord1.xyz = GetWorldSpaceViewDir(p);

    PackedVaryings packedOutput = PackVaryings(output);
    return packedOutput;
}

float4 frag(PackedVaryings packedInput) : SV_TARGET
{
    Varyings unpacked = UnpackVaryings(packedInput);

    UNITY_SETUP_INSTANCE_ID(unpacked);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);

    SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

    return float4(surfaceDescription.BaseColor, surfaceDescription.Alpha);
}