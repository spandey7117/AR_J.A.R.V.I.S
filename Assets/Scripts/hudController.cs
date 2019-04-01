using System.Collections.Generic;
using UnityEngine;

public class HudController : MonoBehaviour {


    public GameObject DisplayPrefab;



    public void CreateNewScreen() {
        Debug.Log("Inside CreateNewScreen");
        GameObject newDisplay = Instantiate(DisplayPrefab);
        DisplayBehavior display = newDisplay.GetComponent<DisplayBehavior>();
        display.SetSelected(true);
    }



    private void Start()
    {
        CreateNewScreen();
    }

}
