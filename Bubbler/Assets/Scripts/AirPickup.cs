using UnityEngine;

public class AirPickup : MonoBehaviour
{
    //modify to increase air added
    public static int airAdded = 6;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player Enter");
           gameObject.SetActive(false);
        }
    }
}
