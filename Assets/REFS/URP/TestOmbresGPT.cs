using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PlanetShadowsFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PlanetShadowSettings
    {
        public Shader planetShadowShader;
        public string planetTag = "PlanetCaster"; // Tag des planètes dans la scène
        public LayerMask occluderLayer = ~0;      // (Optionnel) calque des occludeurs
    }

    public PlanetShadowSettings settings = new PlanetShadowSettings();

    // Données par planète
    private class PlanetData
    {
        public Vector3 centerWS;
        public float radius;
        public ComputeBuffer triBuffer;
        public int triangleCount;
    }

    // Liste des planètes détectées
    private PlanetData[] planets = new PlanetData[0];
    private Material fullscreenMaterial;
    private int _MainLightDirID = Shader.PropertyToID("_LightDirection");

    // Pass custom
    private PlanetShadowsPass renderPass;

    // Structure triangle avec normal pré-calculée
    struct Triangle { public Vector3 v0, v1, v2, n; }
    public override void Create()
    {
        
        // Initialisation du material (shader fullscreen pour appliquer l’ombre)
        if (settings.planetShadowShader == null)
        {
            Debug.LogError("PlanetShadowFeature: Shader non assigné.");
            return;
        }
        fullscreenMaterial = CoreUtils.CreateEngineMaterial(settings.planetShadowShader);
        fullscreenMaterial.enableInstancing = false; // pas nécessaire ici

        // Récupère tous les objets avec le tag planètes et prépare les ComputeBuffers
        GameObject[] planetObjects = GameObject.FindGameObjectsWithTag(settings.planetTag);
        planets = new PlanetData[planetObjects.Length];
        for (int i = 0; i < planetObjects.Length; i++)
        {
            var go = planetObjects[i];
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                continue;
            Mesh mesh = meshFilter.sharedMesh;
            // Calcul centre (monde) et rayon
            Vector3 center = go.transform.position;
            // Rayon approx: on utilise la bounding box locale du mesh
            float radiusLocal = mesh.bounds.extents.magnitude;
            float radiusWorld = radiusLocal * go.transform.lossyScale.x;
            // Préparation des données de triangles en espace monde
            Vector3[] vertices = mesh.vertices;
            int[] indices = mesh.triangles;
            int triCount = indices.Length / 3;
            Triangle[] tris = new Triangle[triCount];
            for (int t = 0; t < triCount; ++t)
            {
                // Indices des sommets du triangle t
                int i0 = indices[t * 3];
                int i1 = indices[t * 3 + 1];
                int i2 = indices[t * 3 + 2];
                // Coordonnées des sommets en espace monde
                Vector3 v0 = go.transform.TransformPoint(vertices[i0]);
                Vector3 v1 = go.transform.TransformPoint(vertices[i1]);
                Vector3 v2 = go.transform.TransformPoint(vertices[i2]);
                // Calcul de la normale du triangle (orientation)
                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                tris[t].v0 = v0;
                tris[t].v1 = v1;
                tris[t].v2 = v2;
                tris[t].n = normal;
            }
            // Création du ComputeBuffer et envoi des données (48 octets par triangle)
            ComputeBuffer buffer = new ComputeBuffer(triCount, sizeof(float) * 12);
            buffer.SetData(tris);
            // Stocke les infos dans PlanetData
            planets[i] = new PlanetData
            {
                centerWS = center,
                radius = radiusWorld,
                triBuffer = buffer,
                triangleCount = triCount
            };
        }

        // Crée l’instance du RenderPass custom
        renderPass = new PlanetShadowsPass(fullscreenMaterial, planets);
        // On définit l’injection juste avant le PostProcess (après l’éclairage et les opaques)
        renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Libération des ComputeBuffer
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (planets != null)
        {
            foreach (var pd in planets)
                pd?.triBuffer?.Release();
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        
        // Ne pas exécuter si pas de material ou de pass dispos
        if (fullscreenMaterial == null || renderPass == null)
            return;
        // Met à jour la direction de la lumière principale (pour le shader) chaque frame
        if (renderingData.lightData.mainLightIndex >= 0)
        {
            Light mainLight = renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].light;
            if (mainLight != null && mainLight.type == LightType.Directional)
            {
                // Direction du *rayon* du pixel vers la lumière (lumière directionnelle = inverse de la direction du faisceau)
                Vector3 lightDir = -mainLight.transform.forward;
                fullscreenMaterial.SetVector(_MainLightDirID, new Vector4(lightDir.x, lightDir.y, lightDir.z, 0));
            }
        }
        renderer.EnqueuePass(renderPass);

    }

    // --- ScriptableRenderPass implémentant le RecordRenderGraph ---
    class PlanetShadowsPass : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly PlanetData[] planets;

        // Données pass pour RenderGraph
        private class PassData
        {
            internal TextureHandle srcColor;
            internal TextureHandle dstColor;
            internal TextureHandle depthTex;
            internal ComputeBuffer triBuffer;
            internal Vector3 planetCenter;
            internal float planetRadius;
        }

        public PlanetShadowsPass(Material mat, PlanetData[] planetData)
        {
            this.material = mat;
            this.planets = planetData;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Récupère les textures de frame de l’URP (couleur active et profondeur)&#8203;:contentReference[oaicite:3]{index=3}
            UniversalResourceData resource = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // Ne pas exécuter si le rendu se fait directement dans le backbuffer (cas particulier scène preview)
            if (resource.isActiveTargetBackBuffer)
                return;

            // Prépare un descripteur pour les textures intermédiaires (identiques à la cible couleur)
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;             // pas de MSAA pour textures intermédiaires
            desc.depthBufferBits = 0;         // pas de buffer de profondeur sur nos textures de travail

            // Handles pour la source initiale et suivi de la cible courante au fil des passes
            TextureHandle prevColor = resource.activeColorTexture;
            TextureHandle tempA = default, tempB = default;

            // Si aucune planète, rien à faire
            if (planets == null || planets.Length == 0)
                return;

            // Crée deux textures temporaires (ping-pong) pour accumuler les ombres de plusieurs planètes
            if (planets.Length > 1)
            {
                tempA = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_PlanetShadowsTempA", false);
                tempB = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_PlanetShadowsTempB", false);
            }
            else
            {
                // Une seule planète : on crée une texture temp unique (on utilisera un blit final vers la cible)
                tempA = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_PlanetShadowsTemp", false);
            }

            // Enregistre une passe par planète dans le RenderGraph
            for (int i = 0; i < planets.Length; ++i)
            {
                bool last = (i == planets.Length - 1);
                // Sélectionne la texture destination : ping-pong A/B ou cible finale pour la dernière
                TextureHandle targetColor;
                if (last)
                {
                    if (planets.Length == 1)
                        targetColor = tempA;  // si 1 planète, on écrit d’abord dans tempA (on blitera ensuite)
                    else
                        targetColor = resource.activeColorTexture; // dernière planète : écrit directement dans la cible caméra
                }
                else
                {
                    // Alternance A/B pour sorties intermédiaires
                    targetColor = (i % 2 == 0) ? tempA : tempB;
                }

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("PlanetShadowPass" + i, out var passData))
                {
                    builder.AllowGlobalStateModification(true);
                    // Remplit les données du pass pour cette planète
                    passData.srcColor = prevColor;
                    passData.dstColor = targetColor;
                    passData.depthTex = resource.cameraDepthTexture; // texture de profondeur de la scène (copiée via DepthPrepass ou CopyDepth pass)
                    passData.triBuffer = planets[i].triBuffer;
                    passData.planetCenter = planets[i].centerWS;
                    passData.planetRadius = planets[i].radius;

                    // Déclare les ressources utilisées par le pass (lecture + écriture)&#8203;:contentReference[oaicite:4]{index=4}
                    builder.UseTexture(passData.srcColor, AccessFlags.Read);       // texture couleur d’entrée (lecture seule)
                    builder.UseTexture(passData.depthTex, AccessFlags.Read);      // profondeur en lecture
                    builder.UseTexture(passData.dstColor, AccessFlags.Write);  // rend vers la texture de sortie (index 0)

                    // Pas de culling automatique de ce pass, on le veut toujours exécuté même si résultat non lu directement (ex: dernière écriture sur activeColor) 
                    builder.AllowPassCulling(false);

                    // Fonction de rendu du pass (lambda appelée pour chaque pixel)
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        RasterCommandBuffer cmd = ctx.cmd;
                        // Bind du buffer de triangles de la planète (structuré) pour le shader
                        cmd.SetGlobalBuffer("_Triangles", data.triBuffer);
                        cmd.SetGlobalInt("_TriangleCount", data.triBuffer.count);
                        // Bind des textures source (couleur & depth) pour le shader (Unity le fait via SetGlobalTexture automatiqement)
                        // => le RenderGraph fournit _CameraDepthTexture et le color via data.srcColor déjà lié au material
                        // Configure le target de rendu (normalement déjà configuré par RenderGraph)
                        // On applique le shader fullscreen avec Blitter
                        Blitter.BlitTexture(cmd, data.srcColor, Vector4.one, material, 0);
                        // Note: Le shader lit _CameraDepthTexture et _Triangles pour ombrer data.srcColor et écrire vers data.dstColor.
                    });
                }

                // Le résultat de ce pass devient la source de la prochaine itération
                prevColor = targetColor;
            }

            // Si on n’avait qu’une seule planète, `prevColor` = tempA avec l’ombre appliquée. 
            // Il faut alors copier tempA vers la cible finale (activeColorTexture) car on n’écrit pas directement sur activeColor en une passe.
            if (planets.Length == 1)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("PlanetShadowCopyBack", out var passData))
                {
                    passData.srcColor = prevColor;
                    passData.dstColor = resource.activeColorTexture;
                    builder.UseTexture(passData.srcColor, AccessFlags.Read);
                    builder.UseTexture(passData.dstColor, AccessFlags.Write);
                    builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                    {
                        Blitter.BlitTexture(ctx.cmd, data.srcColor, Vector4.one, material, 0);
                    });
                }
            }
        }
    }
}
