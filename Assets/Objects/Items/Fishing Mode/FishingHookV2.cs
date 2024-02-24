using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FishingHookV2 : MonoBehaviour
{
    [SerializeField] GlobalParametersSO parameters;
    [SerializeField] float waterDrag = 5.0f;

    [System.NonSerialized] public BaitSO Bait;

    /// <summary>
    /// Whether a fish can see the hook.
    /// Eg., if the player cancels the cast,
    /// then, even if the hook is in the water,
    /// a fish should not bite it.
    /// </summary>
    bool _visible = false;
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                VisibilityChangedEvent?.Invoke(value);
            }
        }
    }
    public System.Action<bool> VisibilityChangedEvent;

    /// <summary>
    /// Called when a fish collides with this hook
    /// </summary>
    public System.Action<Fish> OnHook;

    new Collider collider;
    Rigidbody rb;

    /// <summary>
    /// Initial drag of the hook.
    /// When the hook exits water, it resets to this drag
    /// </summary>
    float initialDrag;

    /// <summary>
    /// Currently hooked Fish and corresponding HookedFish
    /// Null if no fish is hooked
    /// </summary>
    Fish fish;
    HookedFish hookedFish;

    void Reset()
    {
        parameters = FindObjectOfType<GlobalParametersSO>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        initialDrag = rb.drag;

        OnHook += OnHookInternal;
    }

    void OnDestroy()
    {
        if (hookedFish)
        {
            hookedFish.UnhookEvent -= Unhook;
        }
    }

    void OnHookInternal(Fish fish)
    {
        this.fish = fish;
        hookedFish = fish.GetComponent<HookedFish>();

        fish.AttachTo(rb);

        collider.enabled = false;
        // since we disable the collider, we don't get any
        // more OnTrigger_ updates, so we should reset the
        // drag here
        rb.drag = initialDrag;

        hookedFish.UnhookEvent += Unhook;
    }

    /// <summary>
    /// Callback for when the Fish issues an Unhook event.
    /// This is invoked, for example, when the rope snaps.
    /// </summary>
    void Unhook()
    {
        hookedFish.UnhookEvent -= Unhook;

        fish.Detach();

        ResetHook();
    }


    #region Physics
    // Physics: only used for applying drag when entering water

    void OnTriggerEnter(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.drag = waterDrag;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (parameters.WaterLayerMask.Contains(other.gameObject.layer))
        {
            rb.drag = initialDrag;
        }
    }
    #endregion

    public void Break()
    {
        collider.enabled = false;
    }

    public void Fix()
    {
        collider.enabled = true;
    }

    public void ResetHook()
    {
        fish = null;
        hookedFish = null;

        collider.enabled = true;

        Visible = false;
    }
}
