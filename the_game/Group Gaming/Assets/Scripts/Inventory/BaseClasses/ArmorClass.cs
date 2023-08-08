using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="new Tool Class", menuName = "Item/Armor/Armor")]
public class ArmorClass : ItemClass
{
    //Dataa vain tälle itemityypille
    [Header("Armor")]

    public int addedArmorPoints = 0;

    public ArmorType armorType;
    public enum ArmorType {
        Chestplate,
        Cloak,
        Shirt
    }

    // public override void Use(PlayerController caller){
    //     //base.Use(caller)
    // }

    public override ArmorClass GetArmor() {return this;}
}
