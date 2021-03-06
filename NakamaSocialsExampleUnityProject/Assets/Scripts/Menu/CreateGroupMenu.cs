using MenuViews;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class CreateGroupMenu : MenuView
    {
        [SerializeField] private InputField nameOfGroup;
        [SerializeField] private InputField descOfGroup;
        [SerializeField] private Toggle isPrivate;
        [SerializeField] private Button createBtn;

        protected override void Init()
        {
            // ReSharper disable once AsyncVoidLambda
            createBtn.onClick.AddListener(async () =>
            {
                await LoginMenu.Client.CreateGroupAsync(LoginMenu.Session, nameOfGroup.text, descOfGroup.text, null,
                    null, !isPrivate.isOn);
                SyncMenuView.ChangeCurrentView<GroupsListMenu>();
            });
        }
    }
}