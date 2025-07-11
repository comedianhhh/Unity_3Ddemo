using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SceneItem;
public class ItemRespawner : MonoBehaviour {

    [SerializeField] ItemSpawnSetting[] itemSpawnSettings;
    Dictionary<SceneItemBase, ItemSpawnSetting> _items = new Dictionary<SceneItemBase, ItemSpawnSetting>();
    void Start() {
        foreach(var item in itemSpawnSettings) {
            SpawnItem(item);
        }
    }
    void SpawnItem(ItemSpawnSetting item) {
        SceneItemBase sceneItem = Instantiate(item.prefab, item.transform.position, Quaternion.identity).GetComponent<SceneItemBase>();
        _items.Add(sceneItem, item);
        sceneItem.OnPickup += SceneItem_OnPickup;
    }

    private void SceneItem_OnPickup(SceneItemBase item) {
        var setting = _items[item];
        _items.Remove(item);
        StartCoroutine(WaitForSpawn(setting));
    }
    IEnumerator WaitForSpawn(ItemSpawnSetting item) {
        yield return new WaitForSeconds(item.cooldown);
        SpawnItem(item);
        yield return null;
    }

    [Serializable]
    public struct ItemSpawnSetting {
        public GameObject prefab;
        public Transform transform;
        public float cooldown;
    }
}
