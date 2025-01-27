Shader "Wanderer/SunTemp"
{
    Properties
    {
        _SunCol ("Sun color", Color) = (1, 1, 1, 1)
        _LightPos ("light pos", Vector) = (0, 0, 0)
    }

    SubShader
    {

        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "Atmosphere drawing"
            
            // Render State
            Cull Off
            Blend SrcColor One, OneMinusSrcAlpha One
            ZTest Off
            ZWrite Off
            
            HLSLPROGRAM
            
            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "fullscreenPassSetup.hlsl"

            float3 _LightPos;
            float3 _SunCol;
            

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float dstToSurface = length(IN.WorldSpacePosition - _WorldSpaceCameraPos);

                
                
                
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = -IN.ray;
                float3 color = SampleSceneColor(IN.ScreenPosition.xy);
                if (dot(rayDir, _WorldSpaceCameraPos - _LightPos)> 0.999999 && dstToSurface < 0)
                {
                    color = float3(0, 0, 1);
                }

                surface.BaseColor = color;
                surface.Alpha = 1;
                return surface;
            }
            
            #include "fullscreenPassRender.hlsl"
            
            ENDHLSL
        }
    }
}