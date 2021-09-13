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

        protected override void Init()
        {
            Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
            // ReSharper disable once AsyncVoidLambda
            connectBtn.onClick.AddListener(async () =>
            {
                Session = await Client.AuthenticateDeviceAsync(
                    $"{userName.text},{userName.text},{userName.text},{userName.text}", userName.text);
                SyncMenuView.ChangeCurrentView<GroupsListMenu>();
            });
        }
    }
}