using UnityEngine;

namespace Wanderer
{
    public class ShadowCasterSync : MonoBehaviour
    {
        public Material ShadowMat;


        void Update()
        {
            MeshRenderer mR = GetComponent<MeshRenderer>();  
            MeshFilter mF = GetComponent<MeshFilter>();

            Material[] mats = new Material[mF.sharedMesh.subMeshCount];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = ShadowMat;
            }
            mR.sharedMaterials = mats;
        }
    }
}