using MenuViews;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class UpdateGroupMenu : MenuView
    {
        private static UpdateGroupMenu _instance;

        private static string _groupId;
        [SerializeField] private InputField nameOfGroup;
        [SerializeField] private InputField descOfGroup;
        [SerializeField] private Toggle isPrivate;
        [SerializeField] private Button updateBtn;

        public static void SetRequirements(IApiGroup group)
        {
            _groupId = group.Id;
            _instance.nameOfGroup.text = group.Name;
            _instance.descOfGroup.text = group.Description;
            _instance.isPrivate.isOn = !group.Open;
        }

        protected override void Init()
        {
            _instance = this;
            // ReSharper disable once AsyncVoidLambda
            updateBtn.onClick.AddListener(async () =>
            {
                await LoginMenu.Client.UpdateGroupAsync(LoginMenu.Session, _groupId, nameOfGroup.text, !isPrivate.isOn,
                    descOfGroup.text);
                SyncMenuView.ChangeCurrentView<GroupsListMenu>();
            });
        }
    }
}