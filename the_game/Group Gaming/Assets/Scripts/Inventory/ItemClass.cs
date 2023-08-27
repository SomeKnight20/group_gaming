using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public abstract class ItemClass : ScriptableObject
// {
//     //Dataa (jaettu) kaikille itemityypeille
//     [Header("Tool")]
//     public string itemName;
//     public Sprite itemIcon;
//     public bool isStackable = true;

//     public abstract ItemClass GetItem();
//     public abstract ToolClass GetTool();
//     public abstract MiscClass GetMisc();
//     public abstract ConsumableClass GetConsumable();
// }

public class ItemClass : ScriptableObject
{
    //Dataa (jaettu) kaikille itemityypeille
    [Header("Tool")]
    public string itemName;

    public string itemSlotType;

    public Sprite itemIcon;
    public bool isStackable = true;

    public string description;

    public virtual void Use(PlayerInventoryController caller){
        Debug.Log("Used item");
    }

    public virtual ItemClass GetItem() {return this;}
    public virtual ToolClass GetTool() {return null;}
    public virtual ToolClass GetSecondary() {return null;}
    public virtual MiscClass GetMisc() {return null;}
    public virtual ConsumableClass GetConsumable() {return null;}
    public virtual ArmorClass GetArmor() {return null;}
    public virtual ToolClass GetPants() {return null;}
    public virtual ToolClass GetHelmet() {return null;}
    public virtual ToolClass GetAccessory() {return null;}
}