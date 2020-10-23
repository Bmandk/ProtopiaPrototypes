using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public float speed;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Object.Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rotation = new Vector3(0f, speed, 0f);
        transform.Rotate(rotation * Time.deltaTime);
    }
}
