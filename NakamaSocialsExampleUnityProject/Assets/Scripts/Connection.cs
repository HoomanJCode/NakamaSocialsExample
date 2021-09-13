using Nakama;
using UnityEngine;

public class Connection : MonoBehaviour
{
    public static Client Client;
    public static ISession Session;

    private async void Awake()
    {
        Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        Session = await Client.AuthenticateDeviceAsync("MyId", "Me");
    }
}