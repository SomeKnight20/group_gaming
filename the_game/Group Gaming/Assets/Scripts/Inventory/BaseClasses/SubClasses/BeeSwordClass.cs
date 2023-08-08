using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="new Tool Class", menuName = "Item/Tool/BeeSword")]
public class BeeSwordClass : ToolClass
{
    public GameObject beeObject;

    public override void Use(PlayerInventoryController caller){
        //base.Use(caller);
        Debug.Log("bee");
        Instantiate(beeObject, caller.transform.position, Quaternion.identity);
    }
}
