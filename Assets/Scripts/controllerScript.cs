using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerScript : MonoBehaviour
{
    public GameObject DisplayPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }
   


    public GameObject  CreateNewScreen()
    {
        Debug.Log("Inside CreateNewScreen");
        GameObject newDisplay = Instantiate(DisplayPrefab);
        DisplayBehavior display = newDisplay.GetComponent<DisplayBehavior>();
        display.SetSelected(true);
     
        return newDisplay;
    }

    public void deleteScreen(GameObject newDisplay)
    {
        Debug.Log("Inside deleteScreen");
        Destroy(newDisplay);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
