using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolderScript : MonoBehaviour
{
    public ItemClass itemiData;

    public ItemClass[] allItems;

    public SpriteRenderer spriteRenderer;

    public InventoryManager inventory;

    private void Start() {
        // inventory.Add(itemiData, 1);

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        // itemiData = allItems[Random.Range(0, allItems.Length)];

        spriteRenderer.sprite = itemiData.itemIcon;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag == "Player")
        {
            inventory.Add(itemiData, 1);
            Destroy(gameObject);
        }
    }
}