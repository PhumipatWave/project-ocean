using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveKeyBindUI : MonoBehaviour
{
    [Header("Move Direction")]
    [SerializeField] private MoveDirection direction;

    [Header("UI")]
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Button rebindButton;

    private KeyBindManager keyBindManager;

    private void Awake()
    {
        keyBindManager = FindFirstObjectByType<KeyBindManager>();
    }

    private void OnEnable()
    {
        if (keyBindManager == null)
            keyBindManager = FindFirstObjectByType<KeyBindManager>();

        KeyBindManager.OnBindingsChanged += UpdateUI;

        if (rebindButton != null)
            rebindButton.onClick.AddListener(StartRebind);

        UpdateUI();
    }

    private void OnDisable()
    {
        KeyBindManager.OnBindingsChanged -= UpdateUI;

        if (rebindButton != null)
            rebindButton.onClick.RemoveListener(StartRebind);
    }

    private void StartRebind()
    {
        if (keyBindManager == null)
            return;

        keyBindManager.RebindMove(
            direction,
            OnRebindComplete
        );
    }

    private void OnRebindComplete(
        string bindingPath,
        bool success)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (keyBindManager == null || keyText == null)
            return;

        keyText.text =
            keyBindManager.GetMoveBindingDisplayString(direction);
    }
}