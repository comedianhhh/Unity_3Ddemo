using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private Rigidbody[] _ragdollRigidbodies;
    private Collider[] _ragdollColliders;

    private Collider _mainCollider;

    private void Awake()
    {
        _mainCollider = GetComponent<Collider>();

        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();
        SetRagdollState(false);
    }

    public void SetRagdollState(bool isRagdoll)
    {
                                                                       
        if (_mainCollider != null && !IsRagdollBoneCollider(_mainCollider))
        {
            _mainCollider.enabled = !isRagdoll;
        }

        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            rb.isKinematic = !isRagdoll;
            rb.useGravity = isRagdoll;   
        }

        foreach (Collider col in _ragdollColliders)
        {
            if (col == _mainCollider) continue;

            col.enabled = isRagdoll; 
        }
        
        Debug.Log($"Ragdoll State set to: {isRagdoll}");
    }
    
    private bool IsRagdollBoneCollider(Collider col)
    {
        foreach (Collider ragdollCol in _ragdollColliders)
        {
            if (col == ragdollCol) return true;
        }
        return false;
    }
    
    [ContextMenu("Enable Ragdoll")]
    public void EnableRagdollDebug()
    {
        SetRagdollState(true);
    }

    [ContextMenu("Disable Ragdoll")]
    public void DisableRagdollDebug()
    {
        SetRagdollState(false);
    }
}
