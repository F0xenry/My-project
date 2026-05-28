using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TripFormManager : MonoBehaviour
{
    // Замените на имя вашей базы данных
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("Окна и Текст")]
    public GameObject formWindow;
    public TMP_Text formTitleText;

    [Header("Поля Ввода (UI)")]
    public TMP_Dropdown routeDropdown;
    public TMP_Dropdown busDropdown;
    public TMP_InputField dateTimeInput;
    public TMP_InputField totalSeatsInput;
    public TMP_InputField priceInput;

    [Header("Ссылка на главную таблицу")]
    public AdminDashboard dashboard;

    // Списки для хранения реальных ID из БД
    private List<int> routeIds = new List<int>();
    private List<int> busIds = new List<int>();

    void Start()
    {
        if (formWindow != null) formWindow.SetActive(false); // Прячем при старте
    }

    /// <summary>
    /// Метод, который вызывается при нажатии на кнопку "+ Добавить рейс"
    /// </summary>
    public void OpenForCreate()
    {
        formTitleText.text = "Добавление нового рейса";

        // Очищаем поля ввода
        dateTimeInput.text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        totalSeatsInput.text = "";
        priceInput.text = "";

        LoadDropdownData(); // Загружаем маршруты и автобусы из MySQL
        formWindow.SetActive(true); // Показываем форму
    }

    /// <summary>
    /// Метод для сохранения (Кнопка "Сохранить")
    /// </summary>
    public void SaveForm()
    {
        // Проверяем, что есть что выбирать
        if (routeDropdown.options.Count == 0 || busDropdown.options.Count == 0) return;

        // Берем реальные ID из списков по индексу выбранного элемента в Dropdown
        int selectedRouteId = routeIds[routeDropdown.value];
        int selectedBusId = busIds[busDropdown.value];

        DateTime depTime = DateTime.Parse(dateTimeInput.text.Trim());
        DateTime arrTime = depTime.AddHours(5);
        int seats = int.Parse(totalSeatsInput.text.Trim());
        decimal price = decimal.Parse(priceInput.text.Trim());

        

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Запрос на создание нового рейса
                string query = @"INSERT INTO trips (route_id, bus_id, departure_time, arrival_time, total_seats, available_seats, price) 
                                 VALUES (@route, @bus, @deptime, @arrtime, @seats, @seats, @price)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@route", selectedRouteId);
                    cmd.Parameters.AddWithValue("@bus", selectedBusId);
                    cmd.Parameters.AddWithValue("@deptime", depTime);
                    cmd.Parameters.AddWithValue("@arrtime", arrTime);
                    cmd.Parameters.AddWithValue("@seats", seats);
                    cmd.Parameters.AddWithValue("@price", price);

                    cmd.ExecuteNonQuery();
                }

                formWindow.SetActive(false); // Закрываем форму
                if (dashboard != null) dashboard.RefreshTripsTable(); // Обновляем таблицу админа
            }
            catch (Exception ex)
            {
                Debug.LogError("[FormError] Ошибка сохранения: " + ex.Message);
            }
        }
    }

    public void CloseForm()
    {
        formWindow.SetActive(false);
    }

    private void LoadDropdownData()
    {
        routeDropdown.ClearOptions();
        busDropdown.ClearOptions();
        routeIds.Clear();
        busIds.Clear();

        List<string> routeOptions = new List<string>();
        List<string> busOptions = new List<string>();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // 1. Загружаем маршруты
                using (MySqlCommand cmd = new MySqlCommand("SELECT id, departure_city, arrival_city FROM routes", conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        routeIds.Add(r.GetInt32("id"));
                        routeOptions.Add($"{r.GetString("departure_city")} → {r.GetString("arrival_city")}");
                    }
                }

                // 2. Загружаем автобусы
                using (MySqlCommand cmd = new MySqlCommand("SELECT id, bus_number, bus_model FROM buses", conn))
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        busIds.Add(r.GetInt32("id"));
                        busOptions.Add($"{r.GetString("bus_number")} ({r.GetString("bus_model")})");
                    }
                }
            }
            catch (Exception ex) { Debug.LogError("Dropdown load error: " + ex.Message); }
        }

        routeDropdown.AddOptions(routeOptions);
        busDropdown.AddOptions(busOptions);
    }
}
