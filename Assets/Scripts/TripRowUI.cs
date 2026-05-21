using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TripRowUI : MonoBehaviour
{
    [Header("Текстовые поля TMP")]
    public TMP_Text idText;
    public TMP_Text routeText;
    public TMP_Text dateTimeText;
    public TMP_Text seatsText;
    public TMP_Text busText;
    public TMP_Text priceText;

    [Header("Кнопки Управления")]
    public Button editButton;
    public Button deleteButton;

    private int currentTripId;
    private AdminDashboard dashboardManager;

    /// <summary>
    /// Инициализация строки данными рейса
    /// </summary>
    public void SetupRow(TripData data, AdminDashboard manager)
    {
        currentTripId = data.id;
        dashboardManager = manager; // Запоминаем ссылку на главный менеджер для вызова удаления

        if (idText != null) idText.text = $"#{data.id}";
        if (routeText != null) routeText.text = $"{data.routeName}\n<size=70%>({data.station})</size>";
        if (dateTimeText != null) dateTimeText.text = data.departureTime.ToString("dd.MM.yyyy\nHH:mm");
        if (seatsText != null) seatsText.text = $"{data.bookedSeats} / {data.totalSeats}\n<color=green>({data.availableSeats} св.)</color>";
        if (busText != null) busText.text = data.busNumber;
        if (priceText != null) priceText.text = $"{data.price} Р";

        // Подписываем кнопки на события клика программно, чтобы не настраивать вручную в инспекторе
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(OnEditClicked);
        }
    }

    private void OnDeleteClicked()
    {
        // Вызываем метод удаления в главном менеджере панели
        if (dashboardManager != null)
        {
            dashboardManager.DeleteTrip(currentTripId);
        }
    }

    private void OnEditClicked()
    {
        // Метод для вызова режима редактирования (настроим на следующем шаге)
        Debug.Log($"[Admin] Нажато редактирование рейса #{currentTripId}");
    }
}
