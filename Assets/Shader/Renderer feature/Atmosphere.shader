Shader "Volumetric/Atmosphere"
{
    Properties {
        _Radius ("Radius", Range(0, 1000)) = 100
    }

    SubShader
    {

        Tags { "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Name "Atmosphere drawing"
            
            // Render State
            Cull Off
            Blend One Zero, One Zero
            ZTest Off
            ZWrite Off
            
            HLSLPROGRAM
            
            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "fullscreenPassSetup.hlsl"

            // float2 return (dstToSphere, dstThroughSphere)
            // if Ray origin is inside sphere dstToSphere = 0
            // if ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
            // Credits: SEBASTIAN LAGUE (https://youtu.be/DxfEbulyFcY?si=PZFproiFj7ekzADY)
            float2 raySphere(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir)
            {
                float3 offset = rayOrigin - sphereCenter;
                float a = 1;
                float b = 2 * dot(offset, rayDir);
                float c = dot (offset, offset) - sphereRadius * sphereRadius;
                float d = b * b - 4 * a * c;

                if (d > 0)
                {
                    float s = sqrt(d);
                    float dstToSphereNear = max(0, (-b - s) / (2 * a));
                    float dstToSphereFar = (-b + s) / (2 * a);

                    if (dstToSphereFar >= 0)
                    {
                        return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
                    }
                }

                return float2(Max_float(), 0);
            }


            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float depth = length(IN.WorldSpacePosition - _WorldSpaceCameraPos);
                
                
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = -IN.ray;
                
                float2 hitInfo = raySphere(float3(0, 0, 0), 300, rayOrigin, rayDir);


                //missed atmo
                if (hitInfo.x == Max_float()|| 0>depth - hitInfo.x)
                {
                    surface.BaseColor = SampleSceneColor(IN.ScreenPosition.xy);
                    surface.Alpha = float(1);
                    return surface;
                }

                surface.BaseColor = float3((float(min(hitInfo.y, depth - hitInfo.x)/(300 * 2))).xxx) * (rayDir.rgb * .5 + .5);
                surface.Alpha = float(1);
                return surface;
            }
            
            #include "fullscreenPassRender.hlsl"
            
            ENDHLSL
        }
    }
}