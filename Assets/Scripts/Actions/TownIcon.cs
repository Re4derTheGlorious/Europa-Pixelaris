using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownIcon : MonoBehaviour
{

    public Material townMat;
    public Material fortMat;
    public Material capMat;
    public Material hubMat;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Locate(Vector2 pos)
    {
        transform.position = MapTools.LocalToScale(pos);
    }
}
