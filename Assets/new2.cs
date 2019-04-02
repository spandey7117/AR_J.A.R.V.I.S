using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class new2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ExampleStreaming ex = GameObject.FindWithTag("controller").GetComponent<ExampleStreaming>();
        ex.screen2 = GameObject.FindWithTag("screen2o");


        // display.SetSelected(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
