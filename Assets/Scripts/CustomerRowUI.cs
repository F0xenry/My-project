using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerRowUI : MonoBehaviour
{
    [Header("Текстовые поля (TMP)")]
    public TMP_Text idText;
    public TMP_Text full_nameText;
    public TMP_Text ticketsCountText; // Сколько билетов купил
    public TMP_Text moneySpentText;   // Какую сумму принес

    [Header("Кнопки управления")]
    public Button deleteButton;
    public Button detailsButton;

    private int currentCustomerId;
    private CustomerData rawData;
    private CustomersManager dashboardManager;

    /// <summary>
    /// Инициализация строки данными конкретного клиента
    /// </summary>
    public void SetupRow(CustomerData data, CustomersManager manager)
    {
        currentCustomerId = data.id;
        rawData = data;
        dashboardManager = manager;

        if (idText != null) idText.text = $"#{data.id}";
        if (full_nameText != null) full_nameText.text = $"{data.full_name}\n<size=75%><color=#aaaaaa>@{data.login}</color></size>"; // Красиво выводим ФИО + логин снизу мелким шрифтом
        if (ticketsCountText != null) ticketsCountText.text = $"Поездок: {data.totalTicketsBought} шт.";
        if (moneySpentText != null) moneySpentText.text = $"Доход: {data.totalMoneySpent} Р";

        // Программная привязка кнопки удаления, чтобы не настраивать в инспекторе префаба
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(OnDetailsClicked);
        }
    }

    private void OnDeleteClicked()
    {
        if (dashboardManager != null)
        {
            dashboardManager.DeleteCustomer(currentCustomerId);
        }
    }

    private void OnDetailsClicked()
    {
        if (dashboardManager != null)
        {
            dashboardManager.ShowCustomerDetails(rawData);
        }
    }
}
