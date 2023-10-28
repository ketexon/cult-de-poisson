using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Ripple : MonoBehaviour
{
    void Start()
    {
        var mr = GetComponent<MeshRenderer>();
        IEnumerator RippleCoroutine()
        {
            while (true)
            {
                Debug.Log("SET");
                mr.material.SetFloat(Shader.PropertyToID("_RippleStartTime"), Time.time);
                yield return new WaitForSeconds(5);
            }
        }

        StartCoroutine(RippleCoroutine());
    }
}
