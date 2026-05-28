// Assets/Scripts/Profile/TicketItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TicketItemUI : MonoBehaviour
{
    [Header("Основные поля")]
    public TMP_Text routeText;
    public TMP_Text dateTimeText;
    public TMP_Text priceText;
    public TMP_Text statusText;

    [Header("Кнопка")]
    public Button actionButton;
    public TMP_Text buttonText;

    private int currentTicketId = 0;

    public void Setup(TicketInfo ticket)
    {
        currentTicketId = ticket.id;

        routeText.text = $"{ticket.from} → {ticket.to}";
        dateTimeText.text = $"{ticket.date}   {ticket.time}";
        priceText.text = $"{ticket.price} Р";
        statusText.text = ticket.status;

        if (ticket.status == "Активен")
        {
            statusText.color = new Color(0, 0.85f, 0.2f);
            actionButton.gameObject.SetActive(true);
            buttonText.text = "Сохранить PDF";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnPDFButtonClick);
        }
        else
        {
            statusText.color = new Color(0, 0.85f, 0.2f);
            actionButton.gameObject.SetActive(true);
            buttonText.text = "Сохранить PDF";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnPDFButtonClick);
        }
    }

    private void OnPDFButtonClick()
    {
        if (currentTicketId == 0) return;

        // Вызываем метод из UserProfileManager
        UserProfileManager manager = FindObjectOfType<UserProfileManager>();
        if (manager != null)
        {
            manager.GeneratePDFForTicket(currentTicketId);
        }
        else
        {
            Debug.LogError("UserProfileManager не найден на сцене!");
        }
    }
}