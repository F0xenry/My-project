using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Контейнер данных клиента (аналог вашего TripData)
public class CustomerData
{
    public int id;
    public string login;
    public int totalTicketsBought;
    public decimal totalMoneySpent;
    public string full_name;
}

public class CustomersManager : MonoBehaviour
{
    // Строка подключения (использует пустой пароль root, как в вашей БД на гитхабе)
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("Настройки UI таблицы")]
    public Transform container;        // Сюда перетаскиваем Content из Scroll View
    public GameObject rowPrefab;       // Префаб строки клиента с компонентом CustomerRowUI

    [Header("Поле Поиска")]
    public TMP_InputField searchInputField; // Инпут для фильтрации клиентов

    [Header("Настройки автообновления")]
    public bool autoRefresh = true;       // Включить/выключить автообновление
    public float refreshInterval = 5f;    // Интервал в секундах

    private List<GameObject> spawnedRows = new List<GameObject>();

    /// <summary>
    /// Метод загрузки и обновления таблицы клиентов
    /// </summary>
    public void RefreshCustomersTable()
    {
        // Очищаем старые строки перед спавном новых
        foreach (var row in spawnedRows) Destroy(row);
        spawnedRows.Clear();

        List<CustomerData> customers = new List<CustomerData>();
        string searchText = searchInputField != null ? searchInputField.text.Trim() : "";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Запрос связывает таблицу users и tickets по user_id. 
                // Игнорируем администраторов (is_admin = 0)
                string query = @"
                    SELECT 
                        u.id, 
                        u.login,
                        u.full_name,
                        COUNT(t.id) AS tickets_count, 
                        IFNULL(SUM(t.total_price), 0) AS money_spent
                    FROM users u
                    LEFT JOIN tickets t ON u.id = t.user_id
                    WHERE u.is_admin = 0";

                // Добавляем фильтрацию, если админ ввел текст в поиск
                if (!string.IsNullOrEmpty(searchText))
                {
                    query += " AND u.full_name LIKE @search";
                    
                }

                query += " GROUP BY u.id ORDER BY money_spent DESC"; // Сортировка: самые активные сверху

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");
                    }

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CustomerData customer = new CustomerData
                            {
                                id = reader.GetInt32("id"),
                                login = reader.GetString("login"),
                                full_name = reader.GetString("full_name"), // Считываем ФИО из БД
                                totalTicketsBought = reader.GetInt32("tickets_count"),
                                totalMoneySpent = reader.GetDecimal("money_spent")
                            };
                            customers.Add(customer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[CustomersManager] Ошибка загрузки клиентов из MySQL: " + ex.Message);
            }
        }

        // Спавним строки в UI
        PopulateUI(customers);
    }

    private void PopulateUI(List<CustomerData> customers)
    {
        foreach (var customer in customers)
        {
            GameObject newRow = Instantiate(rowPrefab, container);
            spawnedRows.Add(newRow);

            CustomerRowUI rowUI = newRow.GetComponent<CustomerRowUI>();
            if (rowUI != null)
            {
                rowUI.SetupRow(customer, this);
            }
        }
    }

    private Coroutine refreshCoroutine;

    // Вызывается автоматически, когда объект панели КЛИЕНТЫ включается на экране
    void OnEnable()
    {
        RefreshCustomersTable(); // Сразу обновляем при открытии

        if (autoRefresh)
        {
            refreshCoroutine = StartCoroutine(AutoRefreshLoop());
        }
    }

    // Вызывается автоматически, когда админ переключается на другую вкладку
    void OnDisable()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
        }
    }

    // Бесконечный цикл обновления, пока открыта вкладка
    private System.Collections.IEnumerator AutoRefreshLoop()
    {
        while (autoRefresh)
        {
            yield return new WaitForSeconds(refreshInterval);

            // Обновляем только если админ сейчас не пишет ничего в поиск (чтобы не сбивать ввод)
            if (searchInputField != null && !searchInputField.isFocused)
            {
                RefreshCustomersTable();
            }
        }
    }

    [Header("Окно подробной информации")]
    public GameObject infoWindow;
    public TMP_Text infoTitleText;
    public TMP_Text infoContentText;

    /// <summary>
    /// Показывает всплывающее окно с полной историей покупок клиента
    /// </summary>
    public void ShowCustomerDetails(CustomerData customer)
    {
        infoTitleText.text = $"Детализация: {customer.full_name} ({customer.login})";

        string details = $"<b>ID клиента:</b> #{customer.id}\n";
        details += $"<b>Всего куплено билетов:</b> {customer.totalTicketsBought} шт.\n";
        details += $"<b>Общая сумма покупок:</b> {customer.totalMoneySpent} Р\n\n";
        details += "<b><size=120%>История поездок:</size></b>\n---------------------------------------\n";
        Debug.Log($"[DEBUG] 1. Начинаем сбор истории для пользователя ID: {customer.id}");

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                Debug.Log("[DEBUG] 2. Попытка открытия соединения с MySQL...");
                conn.Open();
                Debug.Log("[DEBUG] 3. Соединение успешно открыто. Формируем текст SQL-запроса...");

                // Используем LEFT JOIN, чтобы билеты вывелись в любом случае, 
                // даже если данные рейса или маршрута повреждены/удалены в БД
                string query = @"
                    SELECT 
                        t.id AS ticket_id, 
                        t.quantity, 
                        t.total_price, 
                        tr.departure_time, 
                        IFNULL(r.departure_city, 'Неизвестно') AS dep_city, 
                        IFNULL(r.arrival_city, 'Неизвестно') AS arr_city
                    FROM tickets t
                    LEFT JOIN trips tr ON t.trip_id = tr.id
                    LEFT JOIN routes r ON tr.route_id = r.id
                    WHERE t.user_id = @userId
                    ORDER BY t.id DESC"; // Сортируем от новых билетов к старым

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", customer.id);
                    Debug.Log("[DEBUG] 4. Параметры запроса переданы. Выполняем ExecuteReader()...");
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        Debug.Log("[DEBUG] 5. ExecuteReader выполнен успешно. Начинаем чтение строк (Read)...");
                        int counter = 1;
                        while (r.Read())
                        {
                            Debug.Log($"[DEBUG] -> Считываем строку истории №{counter}...");
                            // Безопасное чтение даты: если в БД NULL, ставим текущее время
                            DateTime date = r.IsDBNull(r.GetOrdinal("departure_time"))
                                ? DateTime.Now
                                : r.GetDateTime("departure_time");

                            int ticketId = r.GetInt32("ticket_id");
                            int qty = r.GetInt32("quantity");
                            decimal price = r.GetDecimal("total_price");
                            string depCity = r.GetString("dep_city");
                            string arrCity = r.GetString("arr_city");

                            // Формируем красивую строку для отображения
                            details += $"{counter}. <b>Билет #{ticketId}</b> | Мест: {qty}\n";
                            details += $"   Маршрут: {depCity} → {arrCity}\n";
                            details += $"   Отправление: {date:dd.MM.yyyy в HH:mm} | Сумма: {price} Р\n";
                            details += "---------------------------------------\n";
                            counter++;
                        }
                        Debug.Log($"[DEBUG] 6. Чтение завершено. Всего найдено записей: {counter - 1}");

                        if (counter == 1)
                        {
                            details += "<color=orange>История поездок пуста. У этого клиента еще нет купленных билетов.</color>";
                        }
                    }
                }
                Debug.Log("[DEBUG] 7. SQL-блок полностью отработал без сбоев.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DEBUG-ERROR] Произошел сбой в коде! Ошибка: {ex.Message}\nСтек вызовов: {ex.StackTrace}");
                details += $"<color=red>Ошибка загрузки истории: {ex.Message}</color>";
            }
        }

        Debug.Log("[DEBUG] 8. Передаем итоговый текст в UI TextMeshPro...");
        infoContentText.text = details;
        Debug.Log("[DEBUG] 9. Включаем игровое окно infoWindow на экране.");
        infoWindow.SetActive(true); // Показываем окно подробностей
    }

    public void CloseInfoWindow()
    {
        infoWindow.SetActive(false);
    }


    /// <summary>
    /// Удаление клиента из базы данных
    /// </summary>
    public void DeleteCustomer(int customerId)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Удаляем пользователя (каскадное удаление билетов выполнится, если настроены связи в InnoDB)
                string query = "DELETE FROM users WHERE id = @id AND is_admin = 0";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.ExecuteNonQuery();
                }

                Debug.Log($"[CustomersManager] Клиент #{customerId} успешно удален.");
                RefreshCustomersTable(); // Перерисовываем UI таблицу
            }
            catch (Exception ex)
            {
                Debug.LogError("[CustomersManager] Ошибка при удалении клиента: " + ex.Message);
            }
        }
    }
}
