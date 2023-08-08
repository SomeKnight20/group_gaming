using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="new Tool Class", menuName = "Item/Tool/Tool")]
public class ToolClass : ItemClass
{
    //Dataa vain tälle itemityypille
    [Header("Tool")]
    public ToolType toolType;
    public enum ToolType {
        weapon,
        pickaxe,
        hammer,
        axe
    }

    public override void Use(PlayerInventoryController caller){
        //base.Use(caller)
        Debug.Log("swing");
    }

    public override ToolClass GetTool() {return this;}
}