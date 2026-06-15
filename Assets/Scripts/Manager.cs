using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // Start is called before the first frame update
    ChangeColorOnClick cube;
    void Start()
    {
        cube = FindObjectOfType<ChangeColorOnClick>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeColor()
    {
        cube.OnMouseDown();
    }

}