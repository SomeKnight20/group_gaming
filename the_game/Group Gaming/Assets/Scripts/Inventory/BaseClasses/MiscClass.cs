using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Tool Class", menuName = "Item/Misc")]
public class MiscClass : ItemClass
{
    //Dataa vain tälle itemityypille

    public override void Use(PlayerInventoryController caller){
        //base.Use(caller)
    }

    public override MiscClass GetMisc() {return this;}
}