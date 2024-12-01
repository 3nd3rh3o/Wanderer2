#if UNITY_EDITOR
using System.Linq;
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


    // ONLY FOR FORCED LOD
    void Update()
    {
        csMan?.UpdateSettings(instructions);
        chunks.ToList().ForEach(c => c.Update(LOD));
    }
}
#endif