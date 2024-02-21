using UnityEngine;

[RequireComponent(typeof(Fish))]
public class HookedFish : MonoBehaviour
{
    public System.Action<float> TugEvent;

    protected void Tug(float strength)
    {
        TugEvent?.Invoke(strength);
    }
}