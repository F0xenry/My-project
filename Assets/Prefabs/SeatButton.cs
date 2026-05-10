using UnityEngine;
using UnityEngine.UI;

public class SeatButton : MonoBehaviour
{
    public int seatNumber;
    private ClientManager clientManager;

    public void Setup(int number, ClientManager manager)
    {
        seatNumber = number;
        clientManager = manager;

        GetComponentInChildren<Text>().text = number.ToString();

        GetComponent<Button>().onClick.AddListener(OnClick);


    }

    public void OnClick()
    {
        Debug.Log("Место выбрано:" + seatNumber);
        GetComponent<Image>().color = Color.yellow;
        clientManager.OnSeatSelected();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
