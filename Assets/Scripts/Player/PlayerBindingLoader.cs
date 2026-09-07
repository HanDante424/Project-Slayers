using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBindingLoader : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;


    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogError("[PlayerBindingLoader] PlayerInput이 없습니다.");
            return;
        }

        ApplySavedBindings();
    }


    private void ApplySavedBindings()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogWarning(
                "[PlayerBindingLoader] GameSession이 없습니다."
            );

            return;
        }

        string json =
            GameSession.Instance.Data.controlBindingsJson;

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log(
                "[PlayerBindingLoader] 저장된 키 설정 없음 - 기본값 사용"
            );

            return;
        }

        playerInput.actions.LoadBindingOverridesFromJson(json);

        Debug.Log(
            "[PlayerBindingLoader] 저장된 키 설정을 PlayerInput에 적용"
        );
    }
}