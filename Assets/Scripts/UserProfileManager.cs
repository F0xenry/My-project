
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class UserProfileManager : MonoBehaviour
{
    public TMP_Text userNameText;
    public TMP_Text userEmailText;
    public TMP_Text userPhoneText;
    public Transform contentParent;
    
    public Button activeTabButton;
    public Button historyTabButton;
    [Header("Префаб билета")]
    public GameObject ticketItemPrefab;



    private int CurrentUserID = 0;
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    private void Awake()
    {

       // FindAllUIElements();

    }

    private void FindAllUIElements()
    {
        userNameText = GameObject.Find("UserNameText")?.GetComponent<TMP_Text>();
        userEmailText = GameObject.Find("UserEmailText")?.GetComponent<TMP_Text>();
        userPhoneText = GameObject.Find("UserPhoneText")?.GetComponent<TMP_Text>();

        contentParent = GameObject.Find("Content")?.GetComponent<Transform>();
        //ticketItemPrefab = GameObject.Find("TicketItemPrefab")?.GetComponent<GameObject>(); // если префаб лежит на сцене

        if (ticketItemPrefab == null)
            ticketItemPrefab = Resources.Load<GameObject>("Prefabs/TicketItem"); 

        activeTabButton = GameObject.Find("ActiveTabButton")?.GetComponent<Button>();
        historyTabButton = GameObject.Find("HistoryTabButton")?.GetComponent<Button>();

        Debug.Log("Все UI элементы найдены автоматически");
    }



    private void Start()
    {
        /*
        CurrentUserID = PlayerPrefs.GetInt("CurrentUserID", 0);

        if (CurrentUserID == 0)
        {
            Debug.LogError("User ID не установлен! Зайдите через логин.");
            return;
        }

        LoadUserData();
        ShowActiveTickets(); // по умолчанию — будущие рейсы

        activeTabButton.onClick.AddListener(ShowActiveTickets);
        historyTabButton.onClick.AddListener(ShowHistoryTickets);
        */
    }

    private void OnEnable()
    {
        // Перезагружаем данные каждый раз, когда кабинет становится активным
        RefreshAll();
        

    }


    

    public void RefreshAll()
    {
        CurrentUserID = PlayerPrefs.GetInt("CurrentUserID", 0);

        if (CurrentUserID == 0)
        {
            Debug.LogWarning("User ID не найден!");
            return;
        }

        SetupTabButtons();
        LoadUserData();
        ShowActiveTickets();
        

    }

    private void SetupTabButtons()
    {
        if (activeTabButton != null)
        {
            activeTabButton.onClick.RemoveAllListeners();
            activeTabButton.onClick.AddListener(ShowActiveTickets);
        }

        if (historyTabButton != null)
        {
            historyTabButton.onClick.RemoveAllListeners();
            historyTabButton.onClick.AddListener(ShowHistoryTickets);
        }

        Debug.Log("Кнопки вкладок успешно перепривязаны");
    }

    private void LoadUserData()
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = "SELECT full_name, email, phone FROM users WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", CurrentUserID);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userNameText.text = reader["full_name"].ToString();
                            userEmailText.text = reader["email"].ToString();
                            userPhoneText.text = reader["phone"].ToString();

                            Debug.Log("Данные пользователя успешно загружены");
                        }

                        else
                        {
                            Debug.LogWarning("Пользователь с таким ID не найден в базе");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Ошибка загрузки пользователя: " + ex.Message);
            }
        }
    }

     public void ShowActiveTickets()
     {
         ClearContent();
         LoadTickets(isActive: true);   // будущие билеты
     }

     public void ShowHistoryTickets()
     {
         ClearContent();
         LoadTickets(isActive: false);  // прошедшие билеты
     }

     private void LoadTickets(bool isActive)
     {
        if (userNameText == null || userEmailText == null || userPhoneText == null)
        {
            Debug.LogError("Не найдены текстовые поля для отображения данных пользователя!");
            return;
        }

        ClearContent();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
         {
             try
             {
                 conn.Open();
                 string query = @"
                     SELECT 
                         t.id,
                         r.departure_city,
                         r.arrival_city,
                         t.purchase_date,
                         tr.departure_time,
                         tr.price
                     FROM tickets t
                     JOIN trips tr ON t.trip_id = tr.id
                     JOIN routes r ON tr.route_id = r.id
                     WHERE t.user_id = @userId
                     ORDER BY t.purchase_date DESC, tr.departure_time DESC";

                 using (MySqlCommand cmd = new MySqlCommand(query, conn))
                 {
                     cmd.Parameters.AddWithValue("@userId", CurrentUserID);
                    Debug.Log($"Выполняем запрос для user_id = {CurrentUserID}");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                     {
                        int totalFound = 0;
                        
                        int count = 0;
                         while (reader.Read())
                         {
                            totalFound++;
                            DateTime ticketDate = Convert.ToDateTime(reader["purchase_date"]);
                            string depTime = reader["departure_time"].ToString();
                            //bool ticketIsActive = ticketDate.Date >= DateTime.Today;

                            bool ticketIsActive = IsTicketStillActive(ticketDate, depTime);
                            // Показываем только нужные по вкладке
                            if (ticketIsActive == isActive)
                             {
                                 TicketInfo ticket = new TicketInfo
                                 {
                                     id = Convert.ToInt32(reader["id"]),
                                     from = reader["departure_city"].ToString(),
                                     to = reader["arrival_city"].ToString(),
                                     date = ticketDate.ToString("dd.MM.yyyy"),
                                     time = depTime,
                                     price = Convert.ToDecimal(reader["price"]),
                                     status = ticketIsActive ? "Активен" : "Завершен"
                                 };

                                 CreateTicketItem(ticket);
                                 count++;
                                 Debug.Log($"Создаём карточку №{count}: {ticket.from} → {ticket.to} ({ticket.date})");
                             }
                         }
                         Debug.Log($"Загружено билетов для {(isActive ? "Активные" : "История")}: {count}");
                         Debug.Log($"Итого создано карточек: {count}");
                        Debug.Log($"=== ИТОГО: найдено {totalFound} билетов | Показано в этой вкладке: {count} ===");
                    }
                 }
             }
             catch (Exception ex)
             {
                 Debug.LogError("Ошибка загрузки билетов: " + ex.Message);
             }
         }
     }
    private bool IsTicketStillActive(DateTime ticketDate, string departureTimeStr)
    {
        if (ticketDate.Date > DateTime.Today)
            return true;

        if (ticketDate.Date < DateTime.Today)
            return false;

        // Билет на сегодня — проверяем время
        if (TimeSpan.TryParse(departureTimeStr, out TimeSpan depTime))
        {
            return depTime > DateTime.Now.TimeOfDay;
        }

        return false;
    }



    private void CreateTicketItem(TicketInfo ticket)
    {
        if (ticketItemPrefab == null)
        {
            Debug.LogError("ticketItemPrefab НЕ НАЗНАЧЕН в инспекторе!");
            return;
        }
        if (contentParent == null)
        {
            Debug.LogError("contentParent НЕ НАЗНАЧЕН!");
            return;
        }
        GameObject item = Instantiate(ticketItemPrefab, contentParent);
        TicketItemUI itemUI = item.GetComponent<TicketItemUI>();
        if (itemUI != null)
        {
            itemUI.Setup(ticket);
            Debug.Log("Карточка успешно создана и Setup выполнен");
        }
        else
        {
            Debug.LogError("На префабе ОТСУТСТВУЕТ компонент TicketItemUI!");
        }
    }

    private void ClearContent()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("CurrentUserId");
        Debug.Log("Выход выполнен");
    }

    // билетик
    public void GeneratePDFForTicket(int ticketId)
    {
        if (PDFGenerator.Instance == null)
        {
            Debug.LogError("PDFGenerator не найден!");
            return;
        }

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = @"
                SELECT 
                    t.id as ticket_id,
                    u.full_name as passenger_name,
                    r.departure_city,
                    r.arrival_city,
                    t.purchase_date,
                    tr.departure_time,
                    tr.price,
                    tr.id as trip_id
                FROM tickets t
                JOIN users u ON t.user_id = u.id
                JOIN trips tr ON t.trip_id = tr.id
                JOIN routes r ON tr.route_id = r.id
                WHERE t.id = @ticketId";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ticketId", ticketId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string passengerName = reader["passenger_name"].ToString();
                            int tripId = Convert.ToInt32(reader["trip_id"]);
                            DateTime tripDate = Convert.ToDateTime(reader["purchase_date"]);
                            string departureTime = reader["departure_time"].ToString();
                            decimal price = Convert.ToDecimal(reader["price"]);
                            string departurePlace = reader["departure_city"].ToString();

                            Debug.Log($"Генерация PDF для билета #{ticketId}");

                            string createdPath = PDFGenerator.Instance.CreateTicketPDF(
                                ticketId: ticketId,
                                passengerName: passengerName,
                                tripId: tripId,
                                quantity: 1,                    
                                totalPrice: price,
                                tripDate: tripDate,
                                departureTime: departureTime,
                                departurePlace: departurePlace
                            );
                            Debug.Log($"[TicketSystem] Успех! PDF-документ сохранен: {createdPath}");

                            
                            Application.OpenURL(createdPath);
                        }
                        else
                        {
                            Debug.LogError($"Билет с ID {ticketId} не найден!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Ошибка при получении данных билета: " + ex.Message);
            }
        }
    }
}

[System.Serializable]
public class TicketInfo
{
    public int id;
    public string from;
    public string to;
    public string date;
    public string time;
    public decimal price;
    public string status;
}