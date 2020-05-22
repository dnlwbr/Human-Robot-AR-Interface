using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    //[SerializeField]
    private GameObject cube;

    // Start is called before the first frame update
    void Start()
    {
        cube = gameObject;
        //cube = GameObject.Find("Cube");
    }

    // Update is called once per frame
    void Update()
    {
        cube.transform.Rotate(0, 3, 0);
    }
}
