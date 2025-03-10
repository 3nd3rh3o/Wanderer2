Shader "Hidden/PlanetShadows/FullscreenShadow"
{
    Properties { _MainTex ("Source", 2D) = ""
    _TrianglesCount ("TriangleCount", int) = 0
     } // texture source (couleur avant ombre)
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "PlanetShadowPass"
            // ShaderLab setup pour full-screen quad
            Cull Off
            Blend SrcColor One, OneMinusSrcAlpha One
            ZTest Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment FragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);
            // Texture de profondeur générée par URP (explicitement disponible car URP settings)
            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            // StructuredBuffer contenant les triangles (positions monde + normale)
            StructuredBuffer<float3> _Triangles;
            uint _TrianglesCount;
            // Paramètres globales
            float4 _LightDirection; // (x,y,z) direction du rayon pixel->lumière (w inutilisé)
            float4 _CameraWorldClipMatrix[4]; // matrice pour reprojeter profondeur en monde (fournie par URP)

            struct Attributes { float4 positionCS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            // Vertex shader par défaut (fourni par URP) pour full-screen pass
            Varyings VertDefault(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = IN.positionCS;
                OUT.uv = IN.uv;
                return OUT;
            }

            // Reconstruction de position monde à partir de profondeur
            float3 ReconstructWorldPos(float2 uv, float depth)
            {
                // Transforme UV + depth en position en clip space puis en espace monde
                float4 clip = float4(uv * 2 - 1, depth * 2 - 1, 1);
                float4 world = mul(unity_CameraInvProjection , clip);
                world /= world.w;
                return world.xyz;
            }

            // Intersection rayon-sphère (retourne true si intersection)
            bool RaySphereIntersect(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius, out float tHit)
            {
                // On résout |rayOrigin + t*rayDir - sphereCenter|^2 = radius^2  (équation quadratique)
                float3 OC = rayOrigin - sphereCenter;
                float a = dot(rayDir, rayDir);
                float b = 2.0 * dot(OC, rayDir);
                float c = dot(OC, OC) - sphereRadius * sphereRadius;
                float discriminant = b*b - 4*a*c;
                tHit = 0;
                if (discriminant < 0) return false;
                // Racines t0, t1
                float sqrtD = sqrt(discriminant);
                float t0 = (-b - sqrtD) / (2*a);
                float t1 = (-b + sqrtD) / (2*a);
                // On prend la plus petite t positive
                if (t0 > 0)
                    tHit = t0;
                else if (t1 > 0)
                    tHit = t1;
                else 
                    return false;
                return true;
            }

            // Intersection rayon-triangle (Möller-Trumbore, retourne true si hit trouvé et t)
            bool RayTriangleIntersect(float3 rayOrigin, float3 rayDir, float3 v0, float3 v1, float3 v2, out float t)
            {
                t = 0;
                float3 edge1 = v1 - v0;
                float3 edge2 = v2 - v0;
                float3 pvec = cross(rayDir, edge2);
                float det = dot(edge1, pvec);
                // Backface culling: si det est proche de 0 ou négatif, on ignore (rayon parallèle ou venant du dos)
                if (det < 1e-6) return false;
                float invDet = 1.0 / det;
                float3 tvec = rayOrigin - v0;
                float u = dot(tvec, pvec) * invDet;
                if (u < 0 || u > 1) return false;
                float3 qvec = cross(tvec, edge1);
                float v = dot(rayDir, qvec) * invDet;
                if (v < 0 || u + v > 1) return false;
                // Calcul t (distance le long du rayon)
                t = dot(edge2, qvec) * invDet;
                return (t > 1e-6);
            }

            // Fragment shader : applique ombre projetée
            float4 FragShadow(Varyings IN) : SV_Target
            {
            
                // Lecture couleur source
                float4 srcColor = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, IN.uv);
                // Reconstruit position monde du pixel courant à partir de la profondeur
                float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, IN.uv).r;
                float3 worldPos = ReconstructWorldPos(IN.uv, depth);
                // Prépare le rayon vers la lumière (direction normale pointant vers le Soleil)
                float3 rayDir = normalize(_LightDirection.xyz);
                float3 rayOrigin = worldPos;
                // Centre et rayon de la planète courante (fournis via uniform global, mis à jour par pass)
                // (Noter: si on voulait plusieurs planètes en un pass, il faudrait un tableau de centres/rayons et boucler ici)
                float3 planetCenter = float3(0,0,0); // on passera ces valeurs via uniform global par planète
                float planetRadius = 0;
                // **Filtrage cylindre** : test rapide rayons-sphère
                float tSphere;
                bool hitSphere = RaySphereIntersect(rayOrigin, rayDir, planetCenter, planetRadius, tSphere);
                if (!hitSphere)
                {
                    // Le rayon ne traverse pas la sphère => pas d’ombre de cette planète
                    return srcColor;
                }
                // **Test détaillé** : intersection rayon-maillage
                float closestT = 1e9;
                bool shadow = false;
                // On parcourt tous les triangles du ComputeBuffer (_Triangles)
                // Chaque triangle est stocké consécutivement (3 sommets + on peut déduire la normale ou la stocker)
                // Ici on suppose qu'on a stocké [v0, v1, v2, normal] par triangle
                // => stride de 4 float3 dans _Triangles
                // Nombre total de float3 dans _Triangles = 4 * nbTriangles.
                uint triCount = (_TrianglesCount / 4);  // nombre de triangles
                [fastopt] // optimisation possible pour aider la boucle
                for (uint t = 0; t < triCount; ++t)
                {
                    // Calcul index de base dans le buffer
                    uint baseIndex = t * 4;
                    float3 v0 = _Triangles[baseIndex];
                    float3 v1 = _Triangles[baseIndex + 1];
                    float3 v2 = _Triangles[baseIndex + 2];
                    float3 n  = _Triangles[baseIndex + 3];
                    // **Backface culling** : on ignore les triangles orientés dos au rayon
                    // (si la normale et le rayon ont un angle obtus => triangle non vu du côté de l’incidence)
                    if (dot(n, rayDir) >= 0) 
                        continue;
                    // Test d’intersection rayon-triangle
                    float tHit;
                    if (RayTriangleIntersect(rayOrigin, rayDir, v0, v1, v2, tHit))
                    {
                        if (tHit > 0 && tHit < closestT)
                        {
                            closestT = tHit;
                            shadow = true;
                            // on peut sortir de la boucle si on ne cherche que le premier contact
                            // break;
                        }
                    }
                }
                // Si une intersection a été trouvée, le pixel est dans l’ombre
                
                if (shadow)
                {
                    // Applique l’assombrissement (ici on met en noir, on pourrait moduler)
                    return float4(0,0,0, 1) * srcColor; // conserve alpha d’origine
                }
                else
                {
                    return srcColor;
                }
            }
            ENDHLSL
        }
    }
}
