using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class TicketBookingManager : MonoBehaviour
{
    [Header("Настройки БД")]
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("UI Элементы")]
    public TMP_Dropdown fromDropdown;
    public TMP_Dropdown toDropdown;
    public Button searchButton;

    public Transform tripsContent;
    public GameObject tripCardPrefab;

    [Header("Окно успеха")]
    public GameObject successPanel;
    public TMP_Text successText;

    [Header("Календарь")]
    public GameObject calendarPanel;           // ← Панель календаря
    public CalendarController calendarController;

    private DateTime selectedDate = new DateTime(2026, 5, 11);

    // Метод для открытия календаря
    public void OpenCalendar()
    {
        if (calendarPanel != null)
        {
            calendarPanel.SetActive(true);
            if (calendarController != null)
                calendarController.SetBookingManager(this);
        }
    }

    // Метод, который вызывает CalendarController когда выбрана дата
    public void SetSelectedDate(DateTime date)
    {
        selectedDate = date;
        Debug.Log("Выбрана дата: " + date.ToString("dd.MM.yyyy"));

        // Можно автоматически обновить поиск после выбора даты
        SearchTrips();
    }

    private void Start()
    {
        SetupDirections();
        if (searchButton) searchButton.onClick.AddListener(SearchTrips);

        if (successPanel) successPanel.SetActive(false);
        if (calendarPanel) calendarPanel.SetActive(false);
    }

    // Настраиваем только два направления
    private void SetupDirections()
    {
        fromDropdown.ClearOptions();
        toDropdown.ClearOptions();

        fromDropdown.AddOptions(new List<string> { "Шарыпово", "Красноярск" });
        toDropdown.AddOptions(new List<string> { "Красноярск", "Шарыпово" });
    }

    public void SearchTrips()
    {
        StartCoroutine(SearchTripsCoroutine());
    }

    private IEnumerator SearchTripsCoroutine()
    {
        // Очищаем предыдущие карточки
        foreach (Transform child in tripsContent)
            Destroy(child.gameObject);

        string searchDate = selectedDate.ToString("yyyy-MM-dd");

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                string query = @"SELECT t.id, t.departure_time, t.arrival_time, t.price, 
                                       t.available_seats, r.departure_city, r.arrival_city 
                                FROM trips t 
                                JOIN routes r ON t.route_id = r.id 
                                WHERE r.departure_city = @from 
                                  AND r.arrival_city = @to 
                                  AND DATE(t.departure_time) = @searchDate 
                                  AND t.available_seats > 0 
                                ORDER BY t.departure_time";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@from", fromDropdown.options[fromDropdown.value].text);
                    cmd.Parameters.AddWithValue("@to", toDropdown.options[toDropdown.value].text);
                    cmd.Parameters.AddWithValue("@searchDate", selectedDate);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool hasResults = false;

                        while (reader.Read())
                        {
                            hasResults = true;

                            int tripId = reader.GetInt32("id");
                            string from = reader.GetString("departure_city");
                            string to = reader.GetString("arrival_city");
                            string depTime = reader.GetDateTime("departure_time").ToString("HH:mm");
                            string arrTime = reader.GetDateTime("arrival_time").ToString("HH:mm");
                            decimal price = reader.GetDecimal("price");
                            int seats = reader.GetInt32("available_seats");
                           

                            GameObject card = Instantiate(tripCardPrefab, tripsContent);
                            TicketCardUI cardUI = card.GetComponent<TicketCardUI>();

                            if (cardUI != null)
                            {
                                cardUI.Setup(tripId, from, to, depTime, arrTime, price, seats, this,
                                             selectedDate.ToString("dd MMMM yyyy"));
                            }
                        }

                        if (!hasResults)
                        {
                            Debug.Log($"На {selectedDate.ToString("dd.MM.yyyy")} рейсов по выбранному направлению нет.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Ошибка поиска рейсов: " + ex.Message);
            }
        }

        yield return null;
    }

    public void BuyTickets(int tripId, int quantity)
    {
        StartCoroutine(BuyTicketsCoroutine(tripId, quantity));
    }

    private IEnumerator BuyTicketsCoroutine(int tripId, int quantity)
    {
        int userId = PlayerPrefs.GetInt("CurrentUserID", 1);
        string passengerName = "Неизвестный Пользователь";

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // === ПОЛУЧАЕМ ФИО ИЗ БАЗЫ ===
                string userQuery = "SELECT full_name FROM users WHERE id = @userId";
                using (MySqlCommand cmd = new MySqlCommand(userQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        passengerName = result.ToString();
                }

                // Получаем информацию о рейсе
                string tripQuery = "SELECT price, available_seats FROM trips WHERE id = @tripId";
                decimal price = 0;
                int currentSeats = 0;

                using (MySqlCommand cmd = new MySqlCommand(tripQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            price = r.GetDecimal("price");
                            currentSeats = r.GetInt32("available_seats");
                        }
                    }
                }

                if (currentSeats < quantity)
                {
                    Debug.LogWarning("Недостаточно свободных мест!");
                    yield break;
                }

                decimal totalPrice = price * quantity;

                // Создаём билет
                string insertTicket = @"INSERT INTO tickets 
                (user_id, trip_id, passenger_name, quantity, total_price) 
                VALUES (@userId, @tripId, @name, @qty, @total)";

                int newTicketId;
                using (MySqlCommand cmd = new MySqlCommand(insertTicket, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    cmd.Parameters.AddWithValue("@name", passengerName);
                    cmd.Parameters.AddWithValue("@qty", quantity);
                    cmd.Parameters.AddWithValue("@total", totalPrice);

                    cmd.ExecuteNonQuery();
                    newTicketId = (int)cmd.LastInsertedId;
                }

                // Обновляем свободные места
                string updateQuery = "UPDATE trips SET available_seats = available_seats - @qty WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@qty", quantity);
                    cmd.Parameters.AddWithValue("@id", tripId);
                    cmd.ExecuteNonQuery();
                }

                // ПОЛУЧАЕМ ДАННЫЕ О РЕЙСЕ ДЛЯ PDF (Добавлено, чтобы избежать CS0103)
                DateTime tripDate = DateTime.Now;
                string departureTime = "";
                string departurePlace = "";

                string selectTripQuery = "SELECT departure_time FROM trips WHERE id = @tripId LIMIT 1"; // Убедитесь, что слова 'date' здесь НЕТ
                using (MySqlCommand cmd = new MySqlCommand(selectTripQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    using (MySqlDataReader tripReader = cmd.ExecuteReader())
                    {
                        if (tripReader.Read())
                        {
                            DateTime fullDateTime = tripReader.GetDateTime("departure_time");
                            tripDate = fullDateTime.Date;
                            departureTime = fullDateTime.ToString("HH:mm");
                        }
                    }
                }

                // ПОЛУЧАЕМ ДАННЫЕ О МАРШРУТЕ И РЕЙСЕ ДЛЯ PDF
                tripDate = DateTime.Now;
                string departureTimeStr = "";
                string departurePlaceStr = "";
                int exactAvailableSeats = 0;

                // Объединяем trips и routes по route_id (или как у вас называется поле связи)
                selectTripQuery = @"
                    SELECT t.departure_time, t.available_seats, r.departure_city, r.arrival_city, r.departure_station 
                    FROM trips t
                    INNER JOIN routes r ON t.route_id = r.id 
                    WHERE t.id = @tripId LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(selectTripQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    using (MySqlDataReader tripReader = cmd.ExecuteReader())
                    {
                        if (tripReader.Read())
                        {
                            DateTime fullDateTime = tripReader.GetDateTime("departure_time");
                            tripDate = fullDateTime.Date;
                            departureTimeStr = fullDateTime.ToString("HH:mm");

                            string depCity = tripReader.GetString("departure_city");
                            string arrCity = tripReader.GetString("arrival_city");
                            string station = tripReader.GetString("departure_station");

                            departurePlaceStr = $"{depCity} — {arrCity} ({station})";

                            // Считываем точное число из базы данных прямо сейчас!
                            exactAvailableSeats = tripReader.GetInt32("available_seats");
                        }
                    }
                }

                // Генерируем PDF 
                string pdfPath = PDFGenerator.CreateTicketPDF(
                    newTicketId,
                    passengerName,
                    tripId,
                    quantity,
                    totalPrice,
                    tripDate,
                    departureTimeStr,
                    departurePlaceStr // Передаем автоматически собранный маршрут с остановкой
                );
                // АВТОМАТИЧЕСКОЕ ОТКРЫТИЕ БИЛЕТА (Добавьте этот блок)
                if (!string.IsNullOrEmpty(pdfPath) && System.IO.File.Exists(pdfPath))
                {
                    Debug.Log($"[TicketSystem] Открываем сгенерированный билет: {pdfPath}");
                    Application.OpenURL(pdfPath); // Запуск системного просмотрщика PDF
                    
                }
                else
                {
                    Debug.LogError("[TicketSystem] Не удалось открыть билет: файл не найден или путь пустой.");
                }
                UpdateSpecificCard(tripId, exactAvailableSeats);

                // Показ успеха
                if (successPanel != null)
                {
                    successPanel.SetActive(true);
                    if (successText != null)
                        successText.text = $"Билет #{newTicketId} успешно куплен!\nPDF сохранён в Документы/Билеты ";
                }

                Debug.Log($"Билет #{newTicketId} создан для {passengerName}");
            }
            catch (Exception ex)
            {
                Debug.LogError("Ошибка при покупке: " + ex.Message);
            }
        }
    }


    // Обновляем ТОЛЬКО карточку с нужным tripId
    public void UpdateSpecificCard(int tripIdToUpdate, int finalSeatsCount)
    {
        // Ищем все карточки на сцене
        TicketCardUI[] allCards = FindObjectsOfType<TicketCardUI>();

        foreach (TicketCardUI card in allCards)
        {
            // Проверяем, совпадает ли ID рейса у карточки с обновляемым
            // (В вашем TicketCardUI ID рейса хранится в переменной tripId или внутри объекта данных)
            if (card != null && card.tripId == tripIdToUpdate)
            {
                // Находим текстовое поле мест в вашей карточке и обновляем его напрямую.
                // В вашем скрипте оно может называться availableSeatsText, seatsText или аналогично.
                // Ниже приведен пример прямого обращения к TMP-компоненту вашей карточки:
                if (card.seatsText != null)
                {
                    card.seatsText.text = $"Свободно мест: {finalSeatsCount}";


                }
                if (card.availableSeatsText != null)
                {
                    card.availableSeatsText.text = $"/ {finalSeatsCount}";

                }

                // Если у вас в карточке есть локальная копия данных, обновляем и её
                // card.currentAvailableSeats = finalSeatsCount;

                break; // Карточка найдена и обновлена, выходим из цикла
            }
        }
    }


    public void HideSuccessPanel()
    {
        if (successPanel != null)
            successPanel.SetActive(false);
    }

    public void ExitToLogin()
    {
        SceneManager.LoadScene("LoginScene");
    }
}