using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    //let camera follow target
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float lerpSpeed = 1.0f;
        // public float targetYOffset = 1.0f;

        private Vector3 targetPos;

        private void Start()
        {
            if (target == null) return;

            this.transform.position = new Vector3(target.position.x, target.position.y, this.transform.position.z);
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            targetPos = target.position;
            targetPos.z = this.transform.position.z;
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        }

    }
}
