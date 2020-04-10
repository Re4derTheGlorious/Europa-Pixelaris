using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingFrame : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private GameObject arrow;
    private GameObject mask;
    private RawImage image;

    private bool isBuilt;
    public string type;
    public int level;

    // Start is called before the first frame update
    void Awake()
    {
        mask = transform.GetChild(2).gameObject;
        mask.SetActive(false);

        arrow = transform.GetChild(1).gameObject;
        image = transform.GetChild(0).gameObject.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFrameActive(bool active, bool isOnTop, bool isBuilt)
    {
        gameObject.SetActive(active);
        arrow.SetActive(!isOnTop);
        if (isBuilt)
        {
            mask.SetActive(true);
            this.isBuilt = true;
        }
        else
        {
            mask.SetActive(false);
            this.isBuilt = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mask.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isBuilt)
        {
            mask.SetActive(false);
        }
    }
}
