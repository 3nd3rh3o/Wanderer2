Shader "Fullscreen/Atmosphere"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType" = "Transparent"}
        Blend One One
        ZTest Always
        ZWrite Off

        Pass
        {
            Name "Main"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;

            // Entrée et variables globales
            float4x4 _CamFrustum;    // Frustum de la caméra
            float4x4 _CamToWorld;    // Matrice pour passer en World Space
            sampler2D _MainTex;

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Varyings : sortie du vertex shader vers le fragment shader
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // Position en Clip Space
                float2 uv : TEXCOORD0;           // Coordonnées UV
                float3 ray : TEXCOORD1;          // Rayon directionnel pour le raymarching
            };

            // Vertex Shader
            Varyings vert(Attributes v)
            {
                Varyings o;

                // Index pour le frustrum (à partir de la composante z du vertex)
                half index = v.position.z;

                // Réinitialiser la composante Z
                v.position.z = 0;

                // Transforme en clip space
                o.positionCS = UnityObjectToClipPos(v.position);
                o.uv = v.uv;

                // Calcul du rayon directionnel à partir du frustrum
                o.ray = _CamFrustum[(int)index].xyz;

                // Normalisation : diviser par abs(z) pour conserver la direction
                o.ray /= abs(o.ray.z);

                // Transformation du rayon en World Space
                o.ray = mul((float3x3)_CamToWorld, o.ray);

                return o;
            }

            // Fragment Shader
            half4 frag(Varyings i) : SV_TARGET
            {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
            }
            ENDHLSL
        }
    }
}