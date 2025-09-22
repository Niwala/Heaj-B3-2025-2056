using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.InputSystem;

public class WeakKnight : MonoBehaviour
{
    public VoxelsExplosion explosion;
    public new MeshRenderer renderer;

    public InputAction click;

    private void OnEnable()
    {
        click.Enable();
        click.performed += OnClick;
    }

    private void OnDisable()
    {
        click.Disable();
    }

    private void OnClick(InputAction.CallbackContext obj)
    {
        if (obj.phase == InputActionPhase.Performed)
        {
            renderer.gameObject.SetActive(false);
            explosion.gameObject.SetActive(true);
            Restore();
        }
    }

    private async void Restore()
    {
        await Task.Delay(2000);
        renderer.gameObject.SetActive(true);
        explosion.gameObject.SetActive(false);
    }
}
