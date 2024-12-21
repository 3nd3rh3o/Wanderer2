Shader "Volumetric/Atmosphere"
{
    Properties
    {
        _AtmosphereRadius ("Radius", Range(0, 1000)) = 100
        _numScatteringPoints ("Number of scattering points", Range(1, 100)) = 1
        _numOpticalDepthPoints ("Number of Optical Depth points", Range(1, 100)) = 1
        _DensityFallOff ("Atmosphere density falloff", Range(0, 20)) = 1
    }

    SubShader
    {

        Tags { "RenderPipeline" = "UniversalPipeline" }
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

            float3 _PlanetPosition;
            float _AtmosphereRadius;
            float _DensityFallOff;
            float _PlanetRadius;
            float _numScatteringPoints;
            float _numOpticalDepthPoints;
            float3 sunDir;

            



            // float2 return (dstToSphere, dstThroughSphere)
            // if Ray origin is inside sphere dstToSphere = 0
            // if ray misses sphere, dstToSphere = maxValue; dstThroughSphere = 0
            // Credits: SEBASTIAN LAGUE (https://youtu.be/DxfEbulyFcY?si=PZFproiFj7ekzADY)
            float2 raySphere(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir)
            {
                float3 offset = rayOrigin - sphereCenter;
                float a = 1;
                float b = 2 * dot(offset, rayDir);
                float c = dot(offset, offset) - sphereRadius * sphereRadius;
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

            float densityAtPoint(float3 densitySamplePoint)
            {
                
                float heightAboveSurface = length(densitySamplePoint - _PlanetPosition) - _PlanetRadius;
				float height01 = heightAboveSurface / (_AtmosphereRadius - _PlanetRadius);
				float localDensity = exp(-height01 * _DensityFallOff) * (1 - height01);
				return localDensity;
            }

            float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 densitySamplePoint = rayOrigin;
                float stepSize = rayLength / (_numOpticalDepthPoints - 1);
                float opticalDepth = 0;

                for (int i = 0; i < _numOpticalDepthPoints; i++)
                {
                    float localDensity = densityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity * stepSize;
                    densitySamplePoint += rayDir * stepSize;
                }
                return opticalDepth;
            }

            float calculateLight(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (_numScatteringPoints - 1);
                float inScatteredLight = 0;
                for (int i = 0; i < _numScatteringPoints; i++)
                {
                    float sunRayLength = raySphere(_PlanetPosition, _AtmosphereRadius, inScatterPoint, sunDir).y;
                    float sunRayOpticalDepth = opticalDepth(inScatterPoint, sunDir, sunRayLength);
                    float viewRayOpticalDepth = opticalDepth(inScatterPoint, -rayDir, stepSize * i);
                    float transmitance = exp( - (sunRayOpticalDepth + viewRayOpticalDepth));
                    float localDensity = densityAtPoint(inScatterPoint);
                    inScatteredLight += localDensity * transmitance * stepSize;
                    inScatterPoint += rayDir * stepSize;
                }
                return inScatteredLight;
            }

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float dstToSurface = length(IN.WorldSpacePosition - _WorldSpaceCameraPos);
                
                
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = -IN.ray;
                
                float2 hitInfo = raySphere(_PlanetPosition, _AtmosphereRadius, rayOrigin, rayDir);

                float dstToAtmosphere = hitInfo.x;
                float dstThroughAtmosphere = min(hitInfo.y, dstToSurface - dstToAtmosphere);

                //missed atmo
                if (dstToAtmosphere == Max_float()|| 0>dstToSurface - hitInfo.x)
                {
                    surface.BaseColor = SampleSceneColor(IN.ScreenPosition.xy);
                    surface.Alpha = float(1);
                    return surface;
                }
                float3 color = SampleSceneColor(IN.ScreenPosition.xy);
                if (dstThroughAtmosphere > 0)
                {
                    float3 pointInAtmosphere = rayOrigin + rayDir * dstToAtmosphere;
                    float light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere);
                    color += float3(0, 0, 0) * (1-light) + light;
                }


                surface.BaseColor = color;
                surface.Alpha = float(dstThroughAtmosphere/(2*_AtmosphereRadius));
                return surface;
            }
            
            #include "fullscreenPassRender.hlsl"
            
            ENDHLSL
        }
    }
}