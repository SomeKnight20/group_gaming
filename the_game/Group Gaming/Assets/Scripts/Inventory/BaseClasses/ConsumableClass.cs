using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Tool Class", menuName = "Item/Consumable")]
public class ConsumableClass : ItemClass
{
    //Dataa vain tälle itemityypille
    [Header("Consumable")]
    public float healthAdded = 0;

    public override void Use(PlayerInventoryController caller){
        caller.inventory.Consume();
    }

    public override ConsumableClass GetConsumable() {return this;}
}