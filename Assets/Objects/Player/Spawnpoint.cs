using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] public Transform LookRoot;
    [SerializeField] public Transform Camera;

    void Start()
    {
        IEnumerator Coro()
        {
            yield return new WaitForSeconds(5);
            Player.Instance.Movement.Teleport(this);
        }

        StartCoroutine(Coro());
    }
}
