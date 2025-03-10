// This shader fills the mesh shape with a color predefined in the code.
Shader "Wanderer_TEST/PlanetTerrain"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" { }
        _LightDirection ("Light position", Vector) = (0, 0, 0)
        _LightPosition ("Light position", Vector) = (0, 0, 0)
        _Position ("Light position", Vector) = (0, 0, 0)
    }

    // The SubShader block containing the Shader code.
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        LOD 300
        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"



            #define MAX_STEP 10
            

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS : POSITION;
                half3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS : SV_POSITION;
                half3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 shadowCoords : TEXCOORD3;
                float3 pos : POSITION1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            
            uniform float3 _LightPosition;
            uniform float3 _Position;
            //To handle projected shadows
            uniform float3 _OtherPlanetsPos[20];
            uniform float _OtherPlanetsRadius[20];

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END
            

            
            bool SphereIntersect(float3 rayOrigin, float3 rayDir, int i, float max_dist, out float hotSpot)
            {
                float3 ray = rayOrigin;
                float dist;
                float minDist = 1.;
                for (int j = 0; j < MAX_STEP; j++)
                {
                    dist = length(_OtherPlanetsPos[i] - ray) - (_OtherPlanetsRadius[i] * 0.5);
                    minDist = min(minDist, dist);
                    ray += (-rayDir * (abs(dist)));
                }
                if (minDist < 0.004)
                {
                    if (minDist < 0.001) hotSpot = 0.;
                    else hotSpot = 0.2;
                        return true;
                }
                return false;
            }

            float Occluded(float3 rayOrigin, float3 rayDir, float max_dist)
            {
                float hotSpot;
                for (int i = 0; i < 20; i++)
                {
                    if (length(_OtherPlanetsPos[i] - _Position) <= 0.1) continue;
                    if (_OtherPlanetsRadius[i] == -1.) return 1.;
                    if (SphereIntersect(rayOrigin, rayDir, i, max_dist, hotSpot))
                    {
                        return hotSpot;
                    }
                }
                return 1.;
            }
            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.pos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);


                // Convert the vertex position to a position on the shadow map
                float4 shadowCoordinates = TransformWorldToShadowCoord(OUT.pos);
                OUT.shadowCoords = shadowCoordinates;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                // Returning the output.
                return OUT;
            }



            //TODO make it receive shadows.
            // The fragment shader definition.
            half4 frag(Varyings IN) : SV_Target
            {
                float light = dot(normalize((_LightPosition - _Position)), IN.normal);
                float3 rayOrigin = IN.pos;
                float3 rayDir = normalize(_Position - _LightPosition);
                float max_dist = length(_Position - _LightPosition);
                
                float realTimeShadow = MainLightRealtimeShadow(IN.shadowCoords);
                float shadow = min(Occluded(rayOrigin, rayDir, max_dist), realTimeShadow) * light;

                // Defining the color variable and returning it.
                half4 customColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor * shadow;
                return customColor;
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}