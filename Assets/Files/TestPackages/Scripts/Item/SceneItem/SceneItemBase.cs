using System;
using UnityEngine;
namespace SceneItem {
    public class SceneItemBase : MonoBehaviour {
        public event Action<SceneItemBase> OnPickup;
        [ContextMenu("Picked Up")]
        public void PickedUp() {
            OnPickup?.Invoke(this);
            Destroy(gameObject);
        }
    }
}