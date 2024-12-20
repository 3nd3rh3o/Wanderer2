Shader "Custom/SceneDepthEffect"
{
    Properties { }

    SubShader
    {

        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "Atmosphere drawing"
            
            // Render State
            Cull Off
            Blend Off
            ZTest Off
            ZWrite Off
            
            HLSLPROGRAM
            
            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            
            
            // Defines

            
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            
            
            
            struct Attributes
            {
                uint vertexID : VERTEXID_SEMANTIC;
            };


            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                float4 texCoord1;
            };


            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0 : INTERP0;
                float4 texCoord1 : INTERP1;
            };
            
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);
                output.positionCS = input.positionCS;
                output.texCoord0.xyzw = input.texCoord0;
                output.texCoord1.xyzw = input.texCoord1;
                return output;
            }
            
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.texCoord0.xyzw;
                output.texCoord1 = input.texCoord1.xyzw;
                return output;
            }

            struct SurfaceDescription
            {
                float3 BaseColor;
                float Alpha;
            };
            
            struct FragOutput
            {
                float4 color : SV_TARGET;
            };
            
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float depth = SampleSceneDepth(IN.ScreenPosition.xy);
                surface.BaseColor = (depth.xxx);
                surface.Alpha = float(1);
                return surface;
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                float3 normalWS = SampleSceneNormals(input.texCoord0.xy);
                float4 tangentWS = float4(0, 1, 0, 0); // We can't access the tangent in screen space
                
                
                
                
                float3 viewDirWS = normalize(input.texCoord1.xyz);
                float linearDepth = LinearEyeDepth(SampleSceneDepth(input.texCoord0.xy), _ZBufferParams);
                float3 cameraForward = -UNITY_MATRIX_V[2].xyz;
                float camearDistance = linearDepth / dot(viewDirWS, cameraForward);
                float3 positionWS = viewDirWS * camearDistance + GetCameraPositionWS();
                
                
                output.WorldSpacePosition = positionWS;
                output.ScreenPosition = float4(input.texCoord0.xy, 0, 1);
                return output;
            }

            

            float4 GetDrawProceduralVertexPosition(uint vertexID)
            {
                return GetFullScreenTriangleVertexPosition(vertexID, UNITY_NEAR_CLIP_VALUE);
            }

            void BuildVaryings(Attributes input, inout Varyings output)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.texCoord0 = output.positionCS * 0.5 + 0.5;

                
                
                output.texCoord0.y = 1 - output.texCoord0.y;
                

                float3 p = ComputeWorldSpacePosition(output.positionCS, UNITY_MATRIX_I_VP);

                // Encode view direction in texCoord1
                output.texCoord1.xyz = GetWorldSpaceViewDir(p);
            }
            
            
            FragOutput DefaultFullscreenFragmentShader(PackedVaryings packedInput)
            {
                FragOutput output = (FragOutput)0;
                Varyings unpacked = UnpackVaryings(packedInput);

                UNITY_SETUP_INSTANCE_ID(unpacked);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);

                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

                output.color.rgb = surfaceDescription.BaseColor;
                output.color.a = surfaceDescription.Alpha;
                return output;
            }

            PackedVaryings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = GetDrawProceduralVertexPosition(input.vertexID);
                BuildVaryings(input, output);
                PackedVaryings packedOutput = PackVaryings(output);
                return packedOutput;
            }

            FragOutput frag(PackedVaryings packedInput)
            {
                return DefaultFullscreenFragmentShader(packedInput);
            }
            
            ENDHLSL
        }
    }
}