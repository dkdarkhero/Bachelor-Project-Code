using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveScript : MonoBehaviour
{
    public float cameraSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow) == true)
        {
            gameObject.transform.position += new Vector3(0.1f * cameraSpeed, 0, 0);
        }

        if (Input.GetKey(KeyCode.LeftArrow) == true)
        {
            gameObject.transform.position -= new Vector3(0.1f * cameraSpeed, 0, 0);
        }
    }
}
