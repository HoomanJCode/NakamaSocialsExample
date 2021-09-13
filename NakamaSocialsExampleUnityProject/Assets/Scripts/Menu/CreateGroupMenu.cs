using MenuViews;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class CreateGroupMenu : MenuView
    {
        [SerializeField] private InputField nameOfGroup;
        [SerializeField] private InputField descOfGroup;
        [SerializeField] private Button createBtn;

        protected override void Init()
        {
            // ReSharper disable once AsyncVoidLambda
            createBtn.onClick.AddListener(async () =>
            {
                await Connection.Client.CreateGroupAsync(Connection.Session, nameOfGroup.text, descOfGroup.text);
                SyncMenuView.ChangeCurrentView<GroupsListMenu>();
            });
        }
    }
}