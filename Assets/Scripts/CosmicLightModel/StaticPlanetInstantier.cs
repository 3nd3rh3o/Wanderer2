using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Wanderer
{
    public static class StaticPlanetInstantier
    {

        
        /// <summary>
        /// Used To spawn a planet GameObject, correctly configured.
        /// </summary>
        /// <param name="g">The empty GameObject that will be a planet.</param>
        /// <param name="p">Settings to generate the planet</param>
        /// <param name="transform">Parent transform.</param>
        /// <param name="RLIndex">Index of the render layer to use</param>/// 
        public static void SpawnPlanet(GameObject g, PlanetSettings p, Transform transform, int RLIndex, Material shadowMat)
        {
            g.SetActive(false);
            SetupBaseParams(g, p, transform, RLIndex);
            SetupCosmicLighting(g, RLIndex, shadowMat, transform);
            g.SetActive(true);
        }

        /// <summary>
        /// Assign of base GO properties/components
        /// </summary>
        /// <param name="g"></param>
        /// <param name="p"></param>
        /// <param name="transform"></param>
        private static void SetupBaseParams(GameObject g, PlanetSettings p, Transform transform, int RLIndex)
        {
            MeshRenderer mR = g.AddComponent<MeshRenderer>();
            mR.renderingLayerMask = RenderingLayerMask.GetMask(new string[]{RenderingLayerMask.RenderingLayerToName(RLIndex)});
            MeshFilter mF = g.AddComponent<MeshFilter>();
            mF.sharedMesh = new();
            g.transform.parent = transform;
            g.transform.localPosition = p.position;
            g.transform.localRotation = Quaternion.Euler(p.rotation);
            PlanetEditor e = g.AddComponent<PlanetEditor>();
            e.settings = p;
            e.lv = p.linearVelocity;
            e.DynamicParams = false;
        }
        /// <summary>
        /// Used to spawn neccessary GO to allow for correct shading accros vast distances.
        /// </summary>
        private static void SetupCosmicLighting(GameObject g, int RLIndex, Material shadowMat, Transform parentTransform)
        {
            
            SetupShadowCaster(g, shadowMat);
            
            // add spot
            SetupSpot(g, RLIndex, parentTransform);
        }

        private static void SetupShadowCaster(GameObject g, Material shadowMat)
        {
            GameObject shadowGO = new("ShadowCaster");
            shadowGO.SetActive(false);
            shadowGO.transform.parent = g.transform;
            shadowGO.transform.localPosition = new();
            shadowGO.transform.localScale = new(1, 1, 1);
            shadowGO.transform.localRotation = Quaternion.identity;
            MeshFilter mF = shadowGO.AddComponent<MeshFilter>();
            mF.sharedMesh = g.GetComponent<MeshFilter>().sharedMesh;
            MeshRenderer mR = shadowGO.AddComponent<MeshRenderer>();
            mR.renderingLayerMask = AllMask();
            ShadowCasterSync sCs = shadowGO.AddComponent<ShadowCasterSync>();
            sCs.ShadowMat = shadowMat;
            shadowGO.SetActive(true);
        }

        private static void SetupSpot(GameObject g, int RLIndex, Transform parentTransform)
        {
            GameObject go = new("Spot");
            go.SetActive(false);
            go.transform.parent = parentTransform;
            go.transform.localPosition = new();
            LightSync lS = go.AddComponent<LightSync>();
            lS.sourceTransform = parentTransform;
            lS.targetTransform = g.transform;
            Light l = lS.AddComponent<Light>();
            l.type = LightType.Spot;
            l.intensity = 1000000000;
            l.shadows = LightShadows.Hard;
            l.GetUniversalAdditionalLightData().renderingLayers = IndexToMask(RLIndex);
            
            go.SetActive(true);
        }

        private static uint IndexToMask(int id)
        {
            return RenderingLayerMask.GetMask(new string[]{RenderingLayerMask.RenderingLayerToName(id)});
        }

        private static uint AllMask()
        {
            return RenderingLayerMask.GetMask(RenderingLayerMask.GetDefinedRenderingLayerNames());
        }
    }
}