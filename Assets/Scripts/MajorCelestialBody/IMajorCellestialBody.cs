using UnityEngine;

public class IMajorCellestialBody : MonoBehaviour
{
    public Rigidbody rb;
    public Collider rbCollider;
    public PhysicsMaterial physicMaterial;
    public void Start()
    {
        this.rb = GetComponent<Rigidbody>();
        
    }


    public void Update()
    {
        
    }
}
