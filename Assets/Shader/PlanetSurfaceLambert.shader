Shader "Wanderer/PlanetSurfaceLambert"
{
    Properties
    {
        _MainTex ("test", 2D) = "" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            uniform float4 _Color;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 lightAmount : TEXCOORD2;
                float4 shadowCoords : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                //recover normal in WS
                VertexNormalInputs positions = GetVertexNormalInputs(IN.positionOS.xyz);

                Light light = GetMainLight();

                OUT.lightAmount = LightingLambert(light.color, light.direction, positions.normalWS.xyz);

                // Convert the vertex position to a position on the shadow map
                float4 shadowCoordinates = GetShadowCoord(GetVertexPositionInputs(IN.positionOS.xyz));

                // Pass the shadow coordinates to the fragment shader
                OUT.shadowCoords = shadowCoordinates;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                // Set frag color
                return half4(_Color.rgb * IN.lightAmount * MainLightRealtimeShadow(IN.shadowCoords), 1);
            }
            ENDHLSL
        }
    }
}
