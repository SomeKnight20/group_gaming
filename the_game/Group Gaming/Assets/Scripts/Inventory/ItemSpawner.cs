using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject itemHolder;

    private void Start() {
        Instantiate(itemHolder, gameObject.transform.position, Quaternion.identity);
    }
}
