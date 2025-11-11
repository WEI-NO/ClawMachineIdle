using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GrabButton : InGameButton
{
    public ClawObject clawBody;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI staminaCostText;
    public Image staminaFillBar;

    protected override void OnStart()
    {
        clawBody = ClawObject.Instance;
        clawBody.OnStaminaChange += OnStaminaChange;
        OnStaminaChange(clawBody.CurrentStamina);
    } 

    protected override void ButtonFunction()
    {
        if (animator.GetBool("Disabled")) return;

        ClawObject.Instance.StartGrabSequence();
    }

    void OnStaminaChange(float newStamina)
    {
        float maxStamina = clawBody.MaxStaminaPerRefresh;
        staminaText.text = $"{(int)newStamina}/{(int)maxStamina}";
        staminaFillBar.fillAmount = newStamina / maxStamina;
        staminaCostText.text = $"{-clawBody.StaminaPerGrab}";

        animator.SetBool("Disabled", newStamina < clawBody.StaminaPerGrab);
    }
}
