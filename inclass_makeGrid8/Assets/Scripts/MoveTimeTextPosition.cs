using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTimeTextPosition : MonoBehaviour
{
    public GameObject circle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.parent = circle.transform;
    }
}
