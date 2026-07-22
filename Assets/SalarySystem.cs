using CustomLibrary.References;
using UnityEngine;

public class SalarySystem : MonoBehaviour
{
    public static SalarySystem Instance;

    [Header("Salary Settings")]
    [SerializeField] float salaryCooldown = 30.0f;
    float salaryTimer = 0.0f;
    [SerializeField] string salaryCurrency = "currency001";
    [SerializeField] int salaryCount = 50;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        salaryTimer = salaryCooldown;
    }

    private void Update()
    {
        salaryTimer -= Time.deltaTime;
        if (salaryTimer <= 0.0f)
        {
            PlayerInventory.Instance.GiveItem(salaryCurrency, salaryCount);
            salaryTimer = salaryCooldown;
        }
    }

}
