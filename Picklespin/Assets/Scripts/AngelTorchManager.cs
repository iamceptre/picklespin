using UnityEngine;

public class AngelTorchManager : MonoBehaviour
{
    [SerializeField] private Torch torch;

    private void OnEnable()
    {
        torch.On();
    }

    private void OnDisable()
    {
            torch.Off();
    }
}
