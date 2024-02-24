using UnityEngine;

/// <summary>
/// Base class for behaviors for fish while they are hooked.
/// Disabled by default, but when the fish collides with a hook, it is enabled.
/// Currently, the only thing the fish can do is tug with a certain force, which currently just plays an animation
/// on the fishing rod and adds tension.
/// </summary>
[RequireComponent(typeof(Fish), typeof(FishMovement))]
public class HookedFish : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;

    [System.NonSerialized] public Transform PlayerTransform;
    [System.NonSerialized] public Transform RodTipTransform;

    protected Rigidbody rb;

    public System.Action<float> TugEvent;
    public System.Action OutOfWaterEvent;

    FishMovement fishMovement;

    public System.Action UnhookEvent;

    // whether the fish is in water
    // used to prevent tugging while not in water.
    protected bool inWater = false;

    virtual protected void Awake()
    {
        // just in case its not disabled in editor, disable here
        enabled = false;

        rb = GetComponent<Rigidbody>();
    }

    protected void Tug(float strength)
    {
        if (inWater)
        {
            TugEvent?.Invoke(strength);
        }
    }

    // check if the fish enters water
    void OnTriggerEnter(Collider other)
    {
        if (!inWater && parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            inWater = true;
            if (enabled)
            {
                OnInWater();
            }
        }
    }

    // check if the fish leaves water
    void OnTriggerExit(Collider other)
    {
        
        if(inWater && parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            Debug.Log(other.gameObject);
            inWater = false;
            if (enabled)
            {
                OnOutOfWater();
            }
        }
    }

    virtual protected void OnOutOfWater()
    {
        OutOfWaterEvent?.Invoke();
    }

    virtual protected void OnInWater()
    {
    }

    virtual protected void OnEnable()
    {
        
    }

    virtual protected void OnDisable()
    {
        
    }
}