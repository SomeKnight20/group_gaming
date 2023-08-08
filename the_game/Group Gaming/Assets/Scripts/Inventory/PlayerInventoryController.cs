using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public InventoryManager inventory;
    
    public int addedArmorPoints;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //use item
            if (inventory.primaryItem != null) {
                inventory.primaryItem.Use(this);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            //use item
            if (inventory.secondaryItem != null) {
                inventory.secondaryItem.Use(this);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            //use item
            if (inventory.consumableItem != null) {
                inventory.consumableItem.Use(this);
            }
            
        }

        if (inventory.armorItem != null)
        {
            addedArmorPoints = inventory.armorItem.GetArmor().addedArmorPoints;
        } else {
            addedArmorPoints = 0;
        }

        
    }
}
