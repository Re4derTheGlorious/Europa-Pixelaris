using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{

    private float speedModifier = 8;
    public float defaultSize;
    public float minSize;
    public GameObject map;
    public int cameraLayer = -20;

    void Start()
    {
        transform.position = new Vector3(0, 0, cameraLayer);
        Camera.main.orthographicSize = GameObject.Find("Center").transform.localScale.z*5;
    }

    void Update()
    {
        float speed = Time.deltaTime * speedModifier * Camera.main.orthographicSize *2;

        //Set zoom
        

        

        //Move on a plane
        transform.Translate(new Vector3(Input.GetAxis("Horizontal")*speed, Input.GetAxis("Vertical") * speed, 0));

        //fix x axis
        float currX = transform.position.x;
        float maxX = map.GetComponent<MeshRenderer>().bounds.size.x/2;
        if (currX > maxX)
        {
            transform.position = new Vector3(-maxX, transform.position.y, cameraLayer);
        }
        else if (currX < -maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, cameraLayer);
        }



        //fix y axis
        float currY = transform.position.y;
        float maxY = map.GetComponent<MeshRenderer>().bounds.size.y/2 - Camera.main.orthographicSize;
        if (currY > maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, cameraLayer);
        }
        else if(currY < -maxY)
        {
            transform.position = new Vector3(transform.position.x, -maxY, cameraLayer);
        }

        //Mouse wheel move
        if (Input.GetMouseButton(2))
        {
            Camera.main.transform.position -= new Vector3(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed, 0);
        }
    }

    public void OnMouseScroll()
    {
        float initialZoom = Camera.main.orthographicSize;
        Vector3 zoomPivot = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 shift = zoomPivot - Camera.main.transform.position;

        Camera.main.orthographicSize -= Input.mouseScrollDelta.y * (Camera.main.orthographicSize / (map.GetComponent<MeshRenderer>().bounds.size.y / 2)) * Time.deltaTime * 750;

        if (Camera.main.orthographicSize < minSize)
        {
            Camera.main.orthographicSize = minSize;
        }
        else if (Camera.main.orthographicSize > map.GetComponent<MeshRenderer>().bounds.size.y / 2)
        {
            Camera.main.orthographicSize = map.GetComponent<MeshRenderer>().bounds.size.y / 2;
        }

        float zoomRatio = Camera.main.orthographicSize / initialZoom;
        Vector3 newPos = zoomPivot - shift * zoomRatio;
        newPos.z = cameraLayer;
        Camera.main.transform.position = newPos;
    }

    public void ZoomTo(Vector2 location, int size)
    {
        Camera.main.orthographicSize = size;
        Vector3 newPos = location;
        newPos.z = cameraLayer;
        Camera.main.transform.position = newPos;
    }
}
