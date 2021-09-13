using MenuViews;
using UnityEngine;
using UnityEngine.UI;

public class CreateGroupMenu : MenuView
{
    [SerializeField] private InputField nameOfGroup;
    [SerializeField] private Button createBtn;

    private async void CreateGroup()
    {
        await Connection.Client.CreateGroupAsync(Connection.Session, nameOfGroup.text, "DragonWorld");
        SyncMenuView.ChangeCurrentView<GroupsListMenu>();
    }

    protected override void Init()
    {
        createBtn.onClick.AddListener(CreateGroup);
    }
}