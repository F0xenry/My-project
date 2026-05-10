using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    public GameObject routesPanel;
    public GameObject seatsPanel;
    public GameObject paymentPanel;
    public GameObject seatPrefab;
    public Transform seatParent;

    public void OnRouteSelected()
    {
        seatsPanel.SetActive(true);
        routesPanel.SetActive(false);
    }

    public void OnSeatSelected()
    {
        paymentPanel.SetActive(true);
        seatsPanel.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateSeats(21);
        seatsPanel.SetActive(false);
        paymentPanel.SetActive(false);
        routesPanel.SetActive(true);
    }

    void GenerateSeats(int count)
    {
        for (int i = 1; i < count; i++)
        {
            GameObject seat = Instantiate(seatPrefab, seatParent);
            seat.GetComponent<SeatButton>().Setup(i, this);
            Debug.Log("Создано мест:" + i);
        }

        
    }

    public void Pay()
    {
        Debug.Log("Оплата прошла успешно");
        paymentPanel.SetActive(false);
        routesPanel.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
