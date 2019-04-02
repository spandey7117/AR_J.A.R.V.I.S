using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ExampleStreaming ex = GameObject.FindWithTag("controller").GetComponent<ExampleStreaming>();
        ex.screen1 = GameObject.FindWithTag("screen1o");
      
      
       // display.SetSelected(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
