using System;
using UnityEngine;

public class IMajorCellestialBody : MonoBehaviour
{
    public Rigidbody rb;
    public Collider rbCollider;
    public PhysicsMaterial physicMaterial;
    float radius;
    float mass;
    Vector3 initialVelocity;
    Vector3 initialPosition;
    Vector3 initialTorque;
    Vector3 initialOrientation;
    bool IsKynematic;
    public virtual void Start()
    {
        
    }


    public virtual void Update()
    {
        
    }

    public virtual void Kill()
    {
        if (gameObject!=null) Destroy(gameObject);
    }

    public virtual void OnEnable()
    {
        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(initialOrientation);
        this.rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.isKinematic = IsKynematic;
        rb.linearDamping = 0;
        rb.angularDamping = 0;
        rb.useGravity = false; 
        rb.AddForce(initialVelocity, ForceMode.VelocityChange);
        rb.AddTorque(initialTorque, ForceMode.VelocityChange);
    }

    public virtual void OnDisable()
    {
        rb.isKinematic = true;
        initialVelocity = rb.linearVelocity;
        initialTorque = rb.angularVelocity;
        initialPosition = transform.position;
        initialOrientation = transform.rotation.eulerAngles;
        rb.AddForce(new(), ForceMode.VelocityChange);
        rb.AddTorque(new(), ForceMode.VelocityChange);
    }

    public virtual void Init(
        float radius, float mass,
        Vector3 initialVelocity, Vector3 initialPosition,
        Vector3 initialTorque, Vector3 initialOrientation,
        bool IsKynematic
    )
    {
        this.radius = radius;
        this.mass = mass;
        this.initialVelocity = initialVelocity;
        this.initialPosition = initialPosition;
        this.initialTorque = initialTorque;
        this.initialOrientation = initialOrientation;
        this.IsKynematic = IsKynematic;
    }
}
