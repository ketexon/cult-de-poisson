using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TackleBox : MonoBehaviour
{
    [SerializeField] public int MaxTackles;
    protected List<Tackles> Tackles;
    protected List<Transform> TacklePositions;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void InitializeTacklePositions()
    {
        for(int i = 0; i < MaxTackles; i++)
        {
            Transform transform; 
            transform.position = new Vector3(0, 0, 0);

        }
    }

    public void InitializeTackles()
    {
        for(int i = 0; i < Tackles.Count; i++)
        {
            
        }
    }
}