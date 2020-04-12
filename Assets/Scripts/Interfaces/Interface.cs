using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interface: MonoBehaviour
{
    public bool monthlyTick = false;
    public bool dailyTick = false;
    public bool hourlyTick = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void Refresh();
    public abstract void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null); 
    public abstract bool IsSet();
    public abstract void Disable();
    public abstract void Enable();
    public abstract void MouseInput(Province prov);
    public abstract void KeyboardInput(Province prov);

}
