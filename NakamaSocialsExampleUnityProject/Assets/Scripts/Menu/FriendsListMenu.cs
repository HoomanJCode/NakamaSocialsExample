using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Menu.Items;
using MenuViews;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class FriendsListMenu : MenuView
    {
        private static IEnumerator _enumerator;
        [SerializeField] private Button refreshBtn;
        [SerializeField] private FriendsListItem friendItemTemplate;
        private readonly List<FriendsListItem> _spawnedFriendsUis = new();
        [SerializeField] private InputField username;
        [SerializeField] private Button addFriendBtn;

        protected override void Init()
        {
            // ReSharper disable once AsyncVoidLambda
            addFriendBtn.onClick.AddListener(async () =>
            {
                await LoginMenu.Client.AddFriendsAsync(LoginMenu.Session, null, new[] {username.text});
                Refresh();
            });
            refreshBtn.onClick.AddListener(Refresh);
            friendItemTemplate.gameObject.SetActive(false);
        }

        public override void ChangeToThisView()
        {
            base.ChangeToThisView();
            Refresh();
        }

        private void Refresh()
        {
            if (_enumerator != null)
                EnumeratorRunner.Singletone.StopCoroutine(_enumerator);
            _enumerator = RefreshEnumerator();
            EnumeratorRunner.Singletone.StartCoroutine(_enumerator);
        }

        private IEnumerator RefreshEnumerator()
        {
            //request friends list
            var friendsRequest = LoginMenu.Client.ListFriendsAsync(LoginMenu.Session, null, 100, null);
            //wait
            yield return new WaitUntil(() => friendsRequest.IsCompleted);
            //clear last items
            foreach (var spawnedFriendsUi in _spawnedFriendsUis) Destroy(spawnedFriendsUi.gameObject);
            _spawnedFriendsUis.Clear();
            //spawn new items
            foreach (var friend in friendsRequest.Result.Friends)
            {
                var item = Instantiate(friendItemTemplate.gameObject, friendItemTemplate.transform.parent)
                    .GetComponent<FriendsListItem>();
                item.gameObject.SetActive(true);
                item.UserName = friend.User.Username;
                _spawnedFriendsUis.Add(item);

                async void Ban()
                {
                    await LoginMenu.Client.BlockFriendsAsync(LoginMenu.Session, new[] {friend.User.Id});
                    Refresh();
                }

                async void Add()
                {
                    await LoginMenu.Client.AddFriendsAsync(LoginMenu.Session, new[] {friend.User.Id});
                    Refresh();
                }

                async void Remove()
                {
                    await LoginMenu.Client.DeleteFriendsAsync(LoginMenu.Session, new[] {friend.User.Id});
                    Refresh();
                }

                switch ((FriendStates) friend.State)
                {
                    case FriendStates.AreFriends:
                        item.AddButton("Remove", Remove);
                        item.AddButton("Ban", Ban);
                        break;
                    case FriendStates.InvitedYou:
                        item.AddButton("Accept Invitation", Add);
                        break;
                    case FriendStates.Invited:
                        item.AddButton("Cancel Invitation", Remove);
                        break;
                    case FriendStates.Banned:
                        item.AddButton("Unban", Remove);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}