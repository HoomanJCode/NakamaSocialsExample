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
        [SerializeField] private Button updateBtn;

        public static void SetRequirements(IApiGroup group)
        {
            _groupId = group.Id;
            _instance.nameOfGroup.text = group.Name;
            _instance.descOfGroup.text = group.Description;
        }

        protected override void Init()
        {
            _instance = this;
            // ReSharper disable once AsyncVoidLambda
            updateBtn.onClick.AddListener(async () =>
            {
                await Connection.Client.UpdateGroupAsync(Connection.Session, _groupId, nameOfGroup.text, true,
                    descOfGroup.text);
                SyncMenuView.ChangeCurrentView<GroupsListMenu>();
            });
        }
    }
}