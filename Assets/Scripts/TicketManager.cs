using MySql.Data.MySqlClient;
using System;
using UnityEngine;

public class TicketManager : MonoBehaviour
{
    // Строка подключения (используйте ваши актуальные данные БД)
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    /// <summary>
    /// Главный метод для генерации билета по его ID в вашей базе данных
    /// </summary>
    /// <param name="ticketId">Уникальный ID билета из таблицы билетов</param>
    public void PrintTicketToPDF(int ticketId)
    {
        // 1. Получаем сохраненный при авторизации ID текущего пользователя
        int currentUserId = PlayerPrefs.GetInt("CurrentUserID", 0);
        if (currentUserId == 0)
        {
            Debug.LogError("[TicketSystem] Ошибка: Пользователь не авторизован в системе!");
            return;
        }

        // Переменные для хранения данных из БД
        string passengerName = "";
        int tripId = 0;
        int quantity = 0;
        decimal totalPrice = 0;
        DateTime tripDate = DateTime.Now;
        string departureTime = "";
        string departurePlace = "";

        // 2. Запрос к вашей БД
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Объединяем таблицу билетов, пользователей и рейсов
                // u.login берется как имя пассажира. Если у вас в таблице есть поле FIO — замените u.login на него.
                string query = @"
                    SELECT 
                        t.id AS ticket_id, 
                        u.login AS passenger_name, 
                        t.trip_id, 
                        t.quantity, 
                        t.total_price, 
                        tr.date AS trip_date, 
                        tr.departure_time, 
                        tr.departure_place 
                    FROM tickets t
                    INNER JOIN users u ON t.user_id = u.id
                    INNER JOIN trips tr ON t.trip_id = tr.id
                    WHERE t.id = @ticketId AND t.user_id = @userId 
                    LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ticketId", ticketId);
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            passengerName = reader.GetString("passenger_name");
                            tripId = reader.GetInt32("trip_id");
                            quantity = reader.GetInt32("quantity");
                            totalPrice = reader.GetDecimal("total_price");
                            tripDate = reader.GetDateTime("trip_date");
                            departureTime = reader.GetString("departure_time");
                            departurePlace = reader.GetString("departure_place");
                        }
                        else
                        {
                            Debug.LogWarning($"[TicketSystem] Билет с ID #{ticketId} для текущего пользователя не найден.");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[TicketSystem] Ошибка выполнения SQL-запроса: " + ex.Message);
                return;
            }
        }

        // 3. Передаем собранные данные в ваш готовый и исправленный класс PDFGenerator
        if (tripId != 0)
        {
            string createdPath = PDFGenerator.CreateTicketPDF(
                ticketId,
                passengerName,
                tripId,
                quantity,
                totalPrice,
                tripDate,
                departureTime,
                departurePlace
            );

            if (!string.IsNullOrEmpty(createdPath))
            {
                Debug.Log($"[TicketSystem] Успех! PDF-документ сохранен: {createdPath}");

                // НЕОБЯЗАТЕЛЬНО: Автоматически открыть сгенерированный PDF стандартным просмотрщиком Windows/Mac
                Application.OpenURL(createdPath);
            }
        }
    }
}
