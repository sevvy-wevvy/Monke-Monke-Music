using UnityEngine;
using System;

namespace MonkeMonkeMusic.Scripts
{
    public class Button : MonoBehaviour
    {
        public Action<bool> Click;

        public float Debounce = 0.25f;
        private static float LastPress;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("GorillaInteractable");
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && Time.time > LastPress + Debounce)
            {
                LastPress = Time.time;
                Click?.Invoke(true);
            }
        }
    }
}
