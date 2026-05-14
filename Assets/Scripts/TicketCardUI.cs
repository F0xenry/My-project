using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicketCardUI : MonoBehaviour
{
    [Header("Основная информация")]
    public TMP_Text routeText;           // Шарыпово → Красноярск
    public TMP_Text dateText;            // Дата
    public TMP_Text departureTimeText;   // Время отправления
    public TMP_Text arrivalTimeText;     // Время прибытия
    public TMP_Text priceText;           // 1500 ₽
    public TMP_Text seatsText;           // Свободно: 48 мест

    [Header("Выбор количества")]
    public TMP_InputField quantityInput;
    public TMP_Text availableSeatsText;  // "/ 50"

    [Header("Кнопка")]
    public Button buyButton;

    public int tripId;
    private int maxSeats;
    private TicketBookingManager manager;

    // Основной метод настройки карточки
    public void Setup(int _tripId, string from, string to, string depTime, string arrTime,
                      decimal price, int availableSeats, TicketBookingManager _manager, string date = "")
    {
        tripId = _tripId;
        maxSeats = availableSeats;
        manager = _manager;

        if (routeText) routeText.text = $"{from} → {to}";
        if (dateText && !string.IsNullOrEmpty(date)) dateText.text = date;
        if (departureTimeText) departureTimeText.text = depTime;
        if (arrivalTimeText) arrivalTimeText.text = arrTime;
        if (priceText) priceText.text = $"{price} Р";
        if (seatsText) seatsText.text = $"Свободно: {availableSeats} мест";

        if (availableSeatsText) availableSeatsText.text = $"/ {availableSeats}";

        // Настройка поля количества
        if (quantityInput)
        {
            quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            quantityInput.text = "1";
            quantityInput.onValueChanged.AddListener(OnQuantityChanged);
        }

        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    // Обновляем места ТОЛЬКО если ID рейса совпадает
    public void UpdateSeats(int targetTripId, int newAvailableSeats)
    {
        if (this.tripId != targetTripId)
            return;   // ← Важно! Пропускаем, если это другой рейс

        maxSeats = newAvailableSeats;

        if (seatsText != null)
            seatsText.text = $"Свободно: {newAvailableSeats} мест";

        if (availableSeatsText != null)
            availableSeatsText.text = $"/ {newAvailableSeats}";

        if (buyButton != null)
            buyButton.interactable = newAvailableSeats > 0;
    }

    private void OnQuantityChanged(string value)
    {
        if (int.TryParse(value, out int qty))
        {
            if (qty < 1) quantityInput.text = "1";
            if (qty > maxSeats) quantityInput.text = maxSeats.ToString();
        }
    }

    private void OnBuyClicked()
    {
        if (manager == null) return;

        int quantity = 1;
        if (quantityInput && int.TryParse(quantityInput.text, out int q))
            quantity = Mathf.Clamp(q, 1, maxSeats);

        manager.BuyTickets(tripId, quantity);
    }

   
}