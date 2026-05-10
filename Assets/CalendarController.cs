using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class CalendarController : MonoBehaviour
{
    [Header("UI Элементы")]
    public TMP_Text monthYearText;
    public Transform daysGrid;
    public GameObject dayButtonPrefab;

    public Button prevMonthButton;
    public Button nextMonthButton;
    public Button todayButton;
    public Button closeButton;

    private DateTime currentDate = new DateTime(2026, 5, 1);
    private List<GameObject> dayButtons = new List<GameObject>();

    private TicketBookingManager bookingManager;

    private void Start()
    {
        prevMonthButton.onClick.AddListener(PrevMonth);
        nextMonthButton.onClick.AddListener(NextMonth);
        todayButton.onClick.AddListener(GoToToday);
        closeButton.onClick.AddListener(CloseCalendar);

        GenerateCalendar();
    }

    public void SetBookingManager(TicketBookingManager manager)
    {
        bookingManager = manager;
    }

    private void GenerateCalendar()
    {
        // Очистка старых кнопок
        foreach (var btn in dayButtons) Destroy(btn);
        dayButtons.Clear();

        int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
        DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        int startDay = (int)firstDayOfMonth.DayOfWeek;
        if (startDay == 0) startDay = 7;

        monthYearText.text = currentDate.ToString("MMMM yyyy");

        // Пустые клетки
        for (int i = 1; i < startDay; i++)
        {
            CreateDayButton(0, false, false);
        }

        // Дни месяца
        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime thisDay = new DateTime(currentDate.Year, currentDate.Month, day);
            bool isToday = thisDay.Date == DateTime.Today.Date;

            CreateDayButton(day, true, isToday);
        }
    }

    private void CreateDayButton(int day, bool isActive, bool isToday)
    {
        GameObject btnObj = Instantiate(dayButtonPrefab, daysGrid);
        Button btn = btnObj.GetComponent<Button>();
        TMP_Text text = btnObj.GetComponentInChildren<TMP_Text>();
        Image bg = btnObj.GetComponent<Image>();

        text.text = day.ToString();

        if (isActive)
        {
            btn.interactable = true;
            int selectedDay = day;

            btn.onClick.AddListener(() => OnDateSelected(selectedDay));

            // Выделяем сегодняшний день
            if (isToday)
            {
                bg.color = new Color(0.2f, 0.8f, 1f);     // Ярко-голубой
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;
            }
        }
        else
        {
            btn.interactable = false;
            text.color = new Color(0.6f, 0.6f, 0.6f);
        }

        dayButtons.Add(btnObj);
    }

    private void OnDateSelected(int day)
    {
        DateTime selectedDate = new DateTime(currentDate.Year, currentDate.Month, day);

        Debug.Log($"Выбрана дата: {selectedDate.ToString("dd.MM.yyyy")}"); // Для проверки

        if (bookingManager != null)
        {
            bookingManager.SetSelectedDate(selectedDate);
        }

        // Более надёжное закрытие
        Debug.Log("Закрываем календарь...");
        CloseCalendar();

        // Дополнительная страховка
        gameObject.SetActive(false);
    }


    public void PrevMonth() { currentDate = currentDate.AddMonths(-1); GenerateCalendar(); }
    public void NextMonth() { currentDate = currentDate.AddMonths(1); GenerateCalendar(); }
    public void GoToToday() { currentDate = DateTime.Today; GenerateCalendar(); }

    public void ShowCalendar() { gameObject.SetActive(true); }
    public void CloseCalendar()
    {
        Debug.Log("CloseCalendar вызван");
        gameObject.SetActive(false);
    }
}