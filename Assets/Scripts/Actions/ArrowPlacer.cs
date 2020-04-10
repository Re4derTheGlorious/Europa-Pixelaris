using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPlacer : MonoBehaviour
{
    private Classes.Army army;
    private Province prov;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Classes.Army GetOwner()
    {
        return army;
    }

    // Update is called once per frame
    void Update()
    {
        if(army!=null && prov != null)
        {
            if (army.location == prov)
            {
                if (army.movementStage == 0)
                {
                    Destroy(gameObject);
                }
            }
            else if (army.destProvince == null)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SelfDestructAt(Province prov, Classes.Army army)
    {
        this.prov = prov;
        this.army = army;
    }

    public void Place(Vector2 start, Vector2 end)
    {
        //length
        transform.localScale = new Vector3(0.1f * (end-start).magnitude, 0.1f, 0.1f);

        //distribute children
        //-tail
        transform.GetChild(0).gameObject.transform.localScale = new Vector3(0.1f / transform.localScale.x, 1, 1);
        transform.GetChild(0).gameObject.transform.Translate(new Vector3(-transform.localScale.x*5 - (0.1f / transform.localScale.x), 0, 0));

        //-head
        transform.GetChild(2).gameObject.transform.localScale = new Vector3(0.1f / transform.localScale.x, 1, 1);
        transform.GetChild(2).gameObject.transform.Translate(new Vector3(transform.localScale.x*5 + (0.1f / transform.localScale.x), 0, 0));

        //position and location
        Vector3 startPos = start;
        Vector3 heading = end - start;
        startPos = startPos + heading.normalized * (heading.magnitude * 0.5f);
        startPos.z = -1;
        float angle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.position = startPos;
        transform.rotation = q;
        transform.Rotate(-90, 0, 0);
    }

    public void PlaceTail(Vector2 position)
    {
        Vector3 pos = position;
        pos.z = -1;
        transform.position = pos;
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false);
        transform.Rotate(-90, 0, 0);
    }
}
