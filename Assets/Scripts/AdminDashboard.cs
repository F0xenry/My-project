using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
// Класс-контейнер для удобного хранения данных рейса в Unity
public class TripData
{
    public int id;
    public string routeName;      // Шарыпово - Красноярск
    public string station;        // Остановка
    public DateTime departureTime;// Дата и время
    public int totalSeats;        // Всего мест
    public int bookedSeats;       // Занято мест
    public int availableSeats;    // Свободно мест
    public string busNumber;      // Номер автобуса
    public decimal price;         // Цена билета
}

public class AdminDashboard : MonoBehaviour
{
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("Настройки Таблицы")]
    public Transform container;       // Объект-контейнер (с компонентом Vertical Layout Group)
    public GameObject rowPrefab;      // Префикс строки таблицы (UI элемент)
    public GameObject DelPanel;
    public TMP_Text Deltext;

    private List<GameObject> spawnedRows = new List<GameObject>();

    void Start()
    {
        RefreshTripsTable(); // Автоматически загружаем данные при открытии панели
    }

    /// <summary>
    /// Загружает все рейсы из БД с сортировкой и фильтрацией
    /// </summary>
    public void RefreshTripsTable(string filterMode = "ALL")
    {
        // Очищаем старые строки в интерфейсе перед обновлением
        foreach (var row in spawnedRows) Destroy(row);
        spawnedRows.Clear();

        List<TripData> trips = new List<TripData>();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Сложный SQL-запрос, объединяющий 3 таблицы: Рейсы, Маршруты, Автобусы
                string query = @"
                    SELECT 
                        t.id, 
                        r.departure_city, r.arrival_city, r.departure_station,
                        t.departure_time, t.total_seats, t.available_seats, t.price,
                        b.bus_number
                    FROM trips t
                    INNER JOIN routes r ON t.route_id = r.id
                    INNER JOIN buses b ON t.bus_id = b.id";

                // Динамически меняем сортировку в зависимости от нажатой кнопки слева
                if (filterMode == "ROUTE") query += " ORDER BY r.departure_city ASC";
                else if (filterMode == "TIME") query += " ORDER BY t.departure_time ASC";
                else if (filterMode == "BUS") query += " ORDER BY b.bus_number ASC";
                else query += " ORDER BY t.id DESC"; // По умолчанию ("ВСЕ")

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int total = reader.GetInt32("total_seats");
                        int avail = reader.GetInt32("available_seats");

                        TripData trip = new TripData
                        {
                            id = reader.GetInt32("id"),
                            routeName = $"{reader.GetString("departure_city")} → {reader.GetString("arrival_city")}",
                            station = reader.GetString("departure_station"),
                            departureTime = reader.GetDateTime("departure_time"),
                            totalSeats = total,
                            availableSeats = avail,
                            bookedSeats = total - avail, // Вычисляем занятые места математически
                            busNumber = reader.GetString("bus_number"),
                            price = reader.GetDecimal("price")
                        };
                        trips.Add(trip);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[AdminDashboard] Ошибка загрузки таблицы: " + ex.Message);
            }
        }

        // Отрисовываем полученные данные в UI
        PopulateUI(trips);
    }

    private void PopulateUI(List<TripData> trips)
    {
        foreach (var trip in trips)
        {
            // Создаем новую визуальную строчку из префаба внутри UI контейнера
            GameObject newRow = Instantiate(rowPrefab, container);
            spawnedRows.Add(newRow);

            // Получаем компонент отображения текста из строки (его мы напишем на следующем шаге)
            TripRowUI rowUI = newRow.GetComponent<TripRowUI>();
            if (rowUI != null)
            {
                rowUI.SetupRow(trip, this); // Передаем сам этот менеджер строке билета
            }
        }
    }

    /// <summary>
    /// Физическое удаление рейса из базы данных MySQL
    /// </summary>
    public void DeleteTrip(int tripId)
    {
        // Перед удалением можно вывести системный лог или сделать быструю проверку
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            if (DelPanel != null)
            {
                DelPanel.SetActive(true);
                if (Deltext != null)
                    Deltext.text = $"Рейс #{tripId} успешно удален";
            }
            try
            {
                conn.Open();

                // SQL запрос на удаление строки рейса
                string deleteQuery = "DELETE FROM trips WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", tripId);
                    cmd.ExecuteNonQuery();
                }

                Debug.Log($"[AdminDashboard] Рейс #{tripId} успешно удален из базы данных.");

                // Мгновенно обновляем таблицу на экране, чтобы удаленный рейс исчез
                RefreshTripsTable();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AdminDashboard] Ошибка удаления рейса #{tripId}: " + ex.Message);
            }
        }
    }
}
