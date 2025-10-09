using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Explodable : MonoBehaviour
{
    public GameObject explosion;
    public GameObject model;

    public InputAction explodeKey;

    protected virtual void Awake()
    {
        explodeKey.performed += OnExplodeInput;
    }

    protected virtual void OnEnable()
    {
        explodeKey.Enable();
    }

    protected virtual void OnDisable()
    {
        explodeKey.Disable();
    }

    private void OnExplodeInput(InputAction.CallbackContext obj)
    {
        Explode();
    }

    public void Explode()
    {
        model.SetActive(false);
        explosion.SetActive(true);
    }
}
