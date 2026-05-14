using MySql.Data.MySqlClient;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AdminPanelManager : MonoBehaviour
{
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("UI Поля для создания Маршрута")]
    public InputField departureCityField;
    public InputField arrivalCityField;
    public InputField departureStationField;

    [Header("UI Поля для создания Рейса")]
    public InputField routeIdField;
    public InputField departureTimeField; // Формат ввода: ГГГГ-ММ-ДД ЧЧ:ММ:СС
    public InputField totalSeatsField;
    public InputField priceField;

    [Header("UI Уведомления")]
    public Text statusText;

    /// <summary>
    /// 1. ДОБАВЛЕНИЕ НОВОГО МАРШРУТА (Создает направление между городами)
    /// </summary>
    public void AddNewRoute()
    {
        if (string.IsNullOrEmpty(departureCityField.text) || string.IsNullOrEmpty(arrivalCityField.text))
        {
            ShowStatus("<color=red>Заполните города отправления и прибытия!</color>");
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = @"INSERT INTO routes (departure_city, arrival_city, departure_station) 
                                 VALUES (@depCity, @arrCity, @station)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@depCity", departureCityField.text.Trim());
                    cmd.Parameters.AddWithValue("@arrCity", arrivalCityField.text.Trim());
                    cmd.Parameters.AddWithValue("@station", string.IsNullOrEmpty(departureStationField.text) ? "Автовокзал" : departureStationField.text.Trim());

                    cmd.ExecuteNonQuery();
                    ShowStatus("<color=green>Маршрут успешно добавлен в БД!</color>");
                    ClearRouteFields();
                }
            }
            catch (Exception ex)
            {
                ShowStatus("<color=red>Ошибка БД: " + ex.Message + "</color>");
            }
        }
    }

    /// <summary>
    /// 2. ДОБАВЛЕНИЕ КОНКРЕТНОГО РЕЙСА (Привязывает время и места к маршруту)
    /// </summary>
    public void AddNewTrip()
    {
        if (string.IsNullOrEmpty(routeIdField.text) || string.IsNullOrEmpty(departureTimeField.text))
        {
            ShowStatus("<color=red>Заполните ID маршрута и время!</color>");
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = @"INSERT INTO trips (route_id, departure_time, total_seats, available_seats, price) 
                                 VALUES (@routeId, @depTime, @totalSeats, @availSeats, @price)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@routeId", int.Parse(routeIdField.text));
                    cmd.Parameters.AddWithValue("@depTime", DateTime.Parse(departureTimeField.text)); // Парсим строку в DATETIME
                    cmd.Parameters.AddWithValue("@totalSeats", int.Parse(totalSeatsField.text));
                    cmd.Parameters.AddWithValue("@availSeats", int.Parse(totalSeatsField.text)); // Изначально свободны все места
                    cmd.Parameters.AddWithValue("@price", decimal.Parse(priceField.text));

                    cmd.ExecuteNonQuery();
                    ShowStatus("<color=green>Рейс успешно опубликован!</color>");
                    ClearTripFields();
                }
            }
            catch (FormatException)
            {
                ShowStatus("<color=red>Неверный формат чисел или даты (ГГГГ-ММ-ДД ЧЧ:ММ:СС)!</color>");
            }
            catch (Exception ex)
            {
                ShowStatus("<color=red>Ошибка БД: " + ex.Message + "</color>");
            }
        }
    }

    /// <summary>
    /// 3. УДАЛЕНИЕ РЕЙСА
    /// </summary>
    public void DeleteTrip(int tripId)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM trips WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", tripId);
                    cmd.ExecuteNonQuery();
                    ShowStatus($"<color=orange>Рейс #{tripId} удален.</color>");
                }
            }
            catch (Exception ex)
            {
                ShowStatus("<color=red>Ошибка удаления: " + ex.Message + "</color>");
            }
        }
    }

    private void ShowStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }

    private void ClearRouteFields()
    {
        departureCityField.text = ""; arrivalCityField.text = ""; departureStationField.text = "";
    }

    private void ClearTripFields()
    {
        routeIdField.text = ""; departureTimeField.text = ""; totalSeatsField.text = ""; priceField.text = "";
    }
}
