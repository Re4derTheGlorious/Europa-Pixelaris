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

    //zoom
    private Vector3 targetPosition;
    private float targetZoom;
    private int zoomIteration;
    float speedFactor = 0.02f;

    //drag
    public Vector3 dragPivot;

    void Start()
    {
        transform.position = new Vector3(0, 0, cameraLayer);
        Camera.main.orthographicSize = GameObject.Find("Center").transform.localScale.z*5;
    }

    void Update()
    {
        
    }

    public void CameraInput()
    {
        float speed = Time.deltaTime * speedModifier * Camera.main.orthographicSize * 2;

        //Move on a plane
        if (!Input.GetAxis("Horizontal").Equals(Vector3.zero) || !Input.GetAxis("Vertical").Equals(Vector3.zero))
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                StopZooming();
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                StopZooming();
            }
        }
        transform.Translate(new Vector3(Input.GetAxis("Horizontal") * speed, Input.GetAxis("Vertical") * speed, 0));

        //fix x axis
        float currX = transform.position.x;
        float maxX = map.GetComponent<MeshRenderer>().bounds.size.x / 2;
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
        float maxY = map.GetComponent<MeshRenderer>().bounds.size.y / 2 - Camera.main.orthographicSize;
        if (currY > maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, cameraLayer);
        }
        else if (currY < -maxY)
        {
            transform.position = new Vector3(transform.position.x, -maxY, cameraLayer);
        }

        //Mouse wheel move
        if (Input.GetMouseButtonDown(2))
        {
            dragPivot = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            dragPivot = Camera.main.ScreenToWorldPoint(dragPivot);
        }
        if (Input.GetMouseButton(2))
        {
            
            StopZooming();

            Camera.main.GetComponent<CameraHandler>().OnMouseDrag();
        }


        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            StopZooming();

            Camera.main.GetComponent<CameraHandler>().OnMouseScroll();
        }
    }

    public void OnMouseScroll()
    {
        float initialZoom = Camera.main.orthographicSize;
        Vector3 zoomPivot = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 shift = zoomPivot - Camera.main.transform.position;

        float ortho = Camera.main.orthographicSize - Input.mouseScrollDelta.y * (Camera.main.orthographicSize / (map.GetComponent<MeshRenderer>().bounds.size.y / 2)) * Time.deltaTime * 2500;

        if (ortho < minSize)
        {
            ortho = minSize;
        }
        else if (ortho > map.GetComponent<MeshRenderer>().bounds.size.y / 2)
        {
            ortho = map.GetComponent<MeshRenderer>().bounds.size.y / 2;
        }

        float zoomRatio = ortho / initialZoom;
        Vector3 newPos = zoomPivot - shift * zoomRatio;
        newPos.z = cameraLayer;

        ZoomTo(newPos, ortho, true);
    }
    public void OnMouseDrag()
    {
        Vector3 currentPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        currentPos = Camera.main.ScreenToWorldPoint(currentPos);
        Vector3 offset = dragPivot - currentPos;
        transform.position = transform.position + offset;
    }

    public void ZoomTo(Vector2 location, float size, bool scroll = false)
    {
        targetPosition = location;
        targetZoom = size;
        zoomIteration = 0;
        if (!scroll)
        {
            InvokeRepeating("ZoomAsync", 0, 0.01f);
        }
        else
        {
            InvokeRepeating("ZoomAsync", 0, 0.001f);
        }
    }

    public void ZoomAsync()
    {
        //pos
        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, speedFactor);
        newPos.z = cameraLayer;
        Camera.main.transform.position = newPos;

        //zoom
        Camera.main.orthographicSize += (targetZoom - Camera.main.orthographicSize) * speedFactor;

        if (zoomIteration >= 250)
        {
            CancelInvoke();
        }
        else
        {
            zoomIteration++;
        }
    }

    public void StopZooming()
    {
        CancelInvoke();
    }
}
