using MenuViews;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class MainMenu : MenuView
    {
        [SerializeField] private Button friendsListBtn;
        [SerializeField] private Button groupsListBtn;

        protected override void Init()
        {
            friendsListBtn.onClick.AddListener(ChangeCurrentView<FriendsListMenu>);
            groupsListBtn.onClick.AddListener(ChangeCurrentView<GroupsListMenu>);
        }
    }
}