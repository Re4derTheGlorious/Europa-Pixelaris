using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapMode
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void Enable();
    public abstract void Disable();
    public abstract void Refresh();
}
