using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
        [Header("目标Tag")]
        public string targetTag = "Car";
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(targetTag))
            {
                Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = -1;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(targetTag))
            {
                Rigidbody2D rb = other.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 1;
                }
            }
        }
}
