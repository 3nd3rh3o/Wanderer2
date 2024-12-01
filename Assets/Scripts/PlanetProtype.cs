#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class PlanetPrototype : Planet
{
    private Chunk[] chunks;
    private ChunkNHMapCSManager csMan;
    public int LOD;
    private new void Build()
    {
        base.Build();
    }


    //
    private new void Update()
    {

    }
}
#endif