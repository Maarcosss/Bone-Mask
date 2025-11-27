using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateCameraBoxC : MonoBehaviour
{
    //Autoría o Encargado del Script completo: David Gomez Gines

    private Camera cam;
    private BoxCollider2D camBox;
    private float sizeX, sizeY, ratio;

    // Start is called before the first frame update
    void Start()
    {
        //So this code takes te GO camera, his boxcollider, and uses to calculate the size of the collider
        //in the same size of the aspect ratio :).
        cam = GetComponent<Camera>();
        camBox = GetComponent<BoxCollider2D>();
        //This have to be this way because of a thing, or an error, of unity witch causes in some cases to sizes the collider smaller than the size of the camera.
        sizeY = cam.orthographicSize * 2;
        //Chages to flaot the values of the screen width and heigth.
        ratio = (float)Screen.width / (float)Screen.height;
        //Camera width is what usually changes so x is ecual to Y * ratio.
        sizeX = sizeY * ratio;
        //And this scales the colider.
        camBox.size = new Vector2(sizeX, sizeY);
    }

    // Update is called once per frame
    void Update()
    {
        //I leave this part here in case we need to calculate the size every frame.
        /*sizeY = cam.orthographicSize * 2;
        ratio = (float)Screen.width / (float)Screen.height;
        sizeX = sizeY * ratio;
        camBox.size = new Vector2(sizeX, sizeY);*/
    }
}
