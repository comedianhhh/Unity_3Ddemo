using UnityEngine;
using UsableItem;
using SceneItem;
using System;
using Core;
public class ItemInteractionHandler : MonoBehaviour
{
    [SerializeField]private int _grenadeCount = 0;
    public GameObject HeldGrenadePrefab; 
    public GameObject VariantGrenadePrefab;
    private Health _playerHealth; 
    private float _activeDamageReduction = 0f;
    
    private bool _hasGearEquipped = false;
    
    // Events for UI updates
    public event Action<int> OnGrenadeCountChanged;
    public event Action<bool> OnGearStatusChanged;
    
    
    private Vector3 _debugAimOrigin;
    private Vector3 _debugAimTarget; 
 
    void Awake()
    {
        if (HeldGrenadePrefab == null)
        {
            Debug.LogError("HeldGrenadePrefab is not assigned on ItemInteractionHandler! Cannot throw grenades.");
        }
        if (VariantGrenadePrefab == null)
        {
            Debug.LogWarning("VariantGrenadePrefab is not assigned. Variant throw will use default grenade.");
        }

        _playerHealth = GetComponentInParent<Health>();
        if (_playerHealth == null)
        {
            Debug.LogWarning("Player Health component not found for ItemInteractionHandler. Damage reduction won't apply.");
        }
        
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        SceneItemBase sceneItem = other.GetComponent<SceneItemBase>();
        if (sceneItem != null)
        {
            Gear gearItem = sceneItem.GetComponent<Gear>(); 

            if (gearItem != null) 
            {
                PickUpGear(gearItem, sceneItem);
            }
            else
            {
                PickUpGrenade(sceneItem);
            }
        }
    }

    private void PickUpGrenade(SceneItemBase sceneItemToDestroy)
    {
        int oldCount = _grenadeCount;
        sceneItemToDestroy.PickedUp();
        _grenadeCount++;
        
        OnGrenadeCountChanged?.Invoke(_grenadeCount);
        OnGearStatusChanged?.Invoke(_hasGearEquipped);
        
        // 发布事件
        EventManager.Publish(new GrenadeCountChangedEvent(oldCount, _grenadeCount));
        EventManager.Publish(new ItemPickedUpEvent(sceneItemToDestroy.gameObject, gameObject, "Grenade", transform.position));
        
        Debug.Log($"Picked up a Grenade! Current count: {_grenadeCount}");
    }

    private void PickUpGear(Gear gearItem, SceneItemBase sceneItemToDestroy)
    {
        int oldGrenadeCount = _grenadeCount;
        bool oldGearStatus = _hasGearEquipped;
        
        sceneItemToDestroy.PickedUp();
        _grenadeCount += gearItem.GrenadeCount;
        _hasGearEquipped = gearItem.providesVariantThrow;
        _activeDamageReduction = gearItem.DamageReductionPercentage; // Set active damage reduction

        OnGrenadeCountChanged?.Invoke(_grenadeCount);
        OnGearStatusChanged?.Invoke(_hasGearEquipped);
        
        // 发布事件
        if (oldGrenadeCount != _grenadeCount)
        {
            EventManager.Publish(new GrenadeCountChangedEvent(oldGrenadeCount, _grenadeCount));
        }
        
        if (oldGearStatus != _hasGearEquipped)
        {
            EventManager.Publish(new GearStatusChangedEvent(_hasGearEquipped, "VariantGear"));
        }
        
        EventManager.Publish(new ItemPickedUpEvent(sceneItemToDestroy.gameObject, gameObject, "Gear", transform.position));
        
        Debug.Log($"Picked up Gear! Gained {gearItem.GrenadeCount} grenades. Total: {_grenadeCount}. Variant throw: {_hasGearEquipped}");
    }
    public void TogglePermanentVariantGear()
    {
        _hasGearEquipped = !_hasGearEquipped;
        _activeDamageReduction = _hasGearEquipped ? 0.2f : 0f;

        OnGearStatusChanged?.Invoke(_hasGearEquipped);
        Debug.Log("Permanent Variant Gear Status: " + (_hasGearEquipped ? "EQUIPPED" : "UNEQUIPPED") + $" Damage Reduction: {_activeDamageReduction * 100}%");
    }
    public void ThrowHeldGrenade()
    {
        if (_grenadeCount > 0 && HeldGrenadePrefab != null)
        {
            Debug.Log("Throwing Grenade (instantiating at throw time)!");
            
            Vector3 throwOrigin = transform.position + transform.forward * 0.5f + transform.up * 0.5f;
            Vector3 targetPosition = GetThrowTargetPosition(throwOrigin);

            GameObject newGrenadeGO = null;
            
            // 使用对象池创建手榴弹
            if (Core.GameManager.Instance?.ObjectPool != null)
            {
                if (VariantGrenadePrefab != null && _hasGearEquipped)
                {
                    newGrenadeGO = Core.GameManager.Instance.ObjectPool.Get("VariantGrenade");
                }
                else
                {
                    newGrenadeGO = Core.GameManager.Instance.ObjectPool.Get("Grenade");
                }
                
                // 设置位置和旋转
                newGrenadeGO.transform.position = throwOrigin;
                newGrenadeGO.transform.rotation = Quaternion.identity;
            }
            else
            {
                // 降级到直接实例化
                Debug.LogWarning("Grenade ObjectPool not available");
            }

            Grenade thrownGrenade = newGrenadeGO.GetComponent<Grenade>();

            if (thrownGrenade == null)
            {
                Debug.LogError("HeldGrenadePrefab does not have a Grenade component! Cannot throw.");
                Destroy(newGrenadeGO);
                return;
            }
            
            // 设置手榴弹的目标位置和对象池键名
            thrownGrenade.SetThrowTarget(targetPosition);
            
            // 设置正确的对象池键名
            if (VariantGrenadePrefab != null && _hasGearEquipped)
            {
                thrownGrenade.SetPoolKey("VariantGrenade");
            }
            else
            {
                thrownGrenade.SetPoolKey("Grenade");
            }
            
            int oldCount = _grenadeCount;
            _grenadeCount--; // Decrement grenade count after throwing
            OnGrenadeCountChanged?.Invoke(_grenadeCount);
            
            // 发布事件
            EventManager.Publish(new GrenadeCountChangedEvent(oldCount, _grenadeCount));
            Vector3 throwDirection = (targetPosition - throwOrigin).normalized;
            EventManager.Publish(new WeaponFiredEvent(newGrenadeGO, gameObject, throwOrigin, throwDirection));
            
            Debug.Log($"Grenade thrown! Remaining: {_grenadeCount}");
        }
        else
        {
            Debug.Log("No grenades left to throw!");
        }
    }

    public int ApplyDamageReduction(int incomingDamage)
    {
        float reducedDamage = incomingDamage * (1f - _activeDamageReduction);
        return Mathf.RoundToInt(reducedDamage); // Round to nearest integer damage
    }

    public bool HasGrenadeReady()
    {
        return  _grenadeCount > 0;
    }
    public int GetGrenadeCount()
    {
        return _grenadeCount;
    }

    public bool HasPermanentVariantGearEquipped()
    {
        return _hasGearEquipped;
    }
    
    /// <summary>
    /// 获取投掷目标位置，自动瞄准最近的怪物
    /// </summary>
    private Vector3 GetThrowTargetPosition(Vector3 throwOrigin)
    {
        // 查找所有怪物
        MonsterAI[] allMonsters = FindObjectsByType<MonsterAI>(FindObjectsSortMode.None);
        
        if (allMonsters.Length == 0)
        {
            // 如果没有怪物，投掷到玩家前方
            Vector3 forwardTarget = throwOrigin + transform.forward * 15f;
            Debug.Log("No monsters found, throwing forward");
            return forwardTarget;
        }
        
        MonsterAI nearestMonster = null;
        float nearestDistance = float.MaxValue;
        
        // 找到最近的活着的怪物
        foreach (MonsterAI monster in allMonsters)
        {
            if (monster == null) continue;
            
            // 检查怪物是否还活着
            Health monsterHealth = monster.GetComponent<Health>();
            if (monsterHealth != null && monsterHealth.IsDead) continue;
            
            float distance = Vector3.Distance(throwOrigin, monster.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestMonster = monster;
            }
        }
        
        if (nearestMonster != null)
        {
            // 直接返回怪物的位置作为目标
            Vector3 targetPosition = nearestMonster.transform.position;
            Debug.Log($"Auto-aiming grenade at nearest monster: {nearestMonster.name} (distance: {nearestDistance:F1}m)");
            return targetPosition;
        }
        else
        {
            // 如果没有活着的怪物，投掷到玩家前方
            Vector3 forwardTarget = throwOrigin + transform.forward * 15f;
            Debug.Log("No alive monsters found, throwing forward");
            return forwardTarget;
        }
    }

}