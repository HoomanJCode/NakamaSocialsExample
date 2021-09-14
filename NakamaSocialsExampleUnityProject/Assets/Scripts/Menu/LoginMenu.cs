using MenuViews;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class LoginMenu : MenuView
    {
        public static Client Client;
        public static ISession Session;
        [SerializeField] private InputField userName;
        [SerializeField] private Button connectBtn;
        [SerializeField] private string scheme = "http";
        [SerializeField] private string serverKey = "defaultkey";
        [SerializeField] private string hostIp = "127.0.0.1";
        [SerializeField] private int hostPort = 7350;

        protected override void Init()
        {
            Client = new Client(scheme, hostIp, hostPort, serverKey);
            // ReSharper disable once AsyncVoidLambda
            connectBtn.onClick.AddListener(async () =>
            {
                Session = await Client.AuthenticateDeviceAsync(
                    $"{userName.text},{userName.text},{userName.text},{userName.text}", userName.text);
                SyncMenuView.ChangeCurrentView<MainMenu>();
            });
        }
    }
}