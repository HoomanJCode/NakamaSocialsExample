using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using MenuViews;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable AsyncVoidLambda

namespace Menu
{
    public class GroupsListMenu : MenuView
    {
        [SerializeField] private InputField searchBox;
        [SerializeField] private GroupsListItem groupItemTemplate;
        [SerializeField] private Button createGroup;
        [SerializeField] private Button refreshBtn;
        private readonly List<GroupsListItem> _spawnedItems = new();

        private IEnumerator _refreshEnumerator;

        protected override void Init()
        {
            refreshBtn.onClick.AddListener(Refresh);
            searchBox.onValueChanged.AddListener(_ => Refresh());
            groupItemTemplate.gameObject.SetActive(false);
            createGroup.onClick.AddListener(ChangeCurrentView<CreateGroupMenu>);
        }

        private void Refresh()
        {
            if (_refreshEnumerator != null)
                EnumeratorRunner.Singletone.StopCoroutine(_refreshEnumerator);
            _refreshEnumerator = RefreshEnumerator();
            EnumeratorRunner.Singletone.StartCoroutine(_refreshEnumerator);
        }

        private IEnumerator RefreshEnumerator()
        {
            //get my owned groups or joined groups at first
            var getMyGroupsTask = LoginMenu.Client.ListUserGroupsAsync(LoginMenu.Session, null, 20, null);
            //get other exist groups by search text
            var getAllGroupsTask = LoginMenu.Client.ListGroupsAsync(LoginMenu.Session, $"{searchBox.text}%", 20);
            //wait
            yield return new WaitUntil(() => getMyGroupsTask.IsCompleted && getAllGroupsTask.IsCompleted);
            //clear last list for new items
            foreach (var item in _spawnedItems) Destroy(item.gameObject);
            _spawnedItems.Clear();
            //show my groups
            var myGroups = getMyGroupsTask.Result.UserGroups.ToList();
            myGroups.Sort((x, y) => x.State.CompareTo(y.State));
            foreach (var receivedGroupData in myGroups)
            {
                //spawn ui item
                var item = Instantiate(groupItemTemplate.gameObject, groupItemTemplate.transform.parent)
                    .GetComponent<GroupsListItem>();
                item.Name = receivedGroupData.Group.Name;
                item.Description = receivedGroupData.Group.Description;
                item.MaxMembersCount = receivedGroupData.Group.MaxCount;
                item.IsPrivate = !receivedGroupData.Group.Open;
                item.Rank = (MemberRankAtGroup) receivedGroupData.State;
                item.gameObject.SetActive(true);
                _spawnedItems.Add(item);

                async void Leave()
                {
                    await LoginMenu.Client.LeaveGroupAsync(LoginMenu.Session, receivedGroupData.Group.Id);
                    Refresh();
                }

                void Members()
                {
                    GroupMembersListMenu.SetRequirements(receivedGroupData.Group,
                        (MemberRankAtGroup) receivedGroupData.State);
                    ChangeCurrentView<GroupMembersListMenu>();
                }

                async void CancelRequest()
                {
                    await LoginMenu.Client.LeaveGroupAsync(LoginMenu.Session, receivedGroupData.Group.Id);
                }

                void UpdateGroup()
                {
                    UpdateGroupMenu.SetRequirements(receivedGroupData.Group);
                    ChangeCurrentView<UpdateGroupMenu>();
                }

                async void Remove()
                {
                    await LoginMenu.Client.DeleteGroupAsync(LoginMenu.Session, receivedGroupData.Group.Id);
                    Refresh();
                }

                switch ((MemberRankAtGroup) receivedGroupData.State)
                {
                    case MemberRankAtGroup.SuperAdmin:
                        item.AddButton("Update", UpdateGroup);
                        item.AddButton("Members", Members);
                        item.AddButton("Remove", Remove);
                        break;
                    case MemberRankAtGroup.Admin:

                        item.AddButton("Update", UpdateGroup);
                        item.AddButton("Members", Members);
                        item.AddButton("Leave", Leave);
                        break;
                    case MemberRankAtGroup.Member:
                        item.AddButton("Members", Members);
                        item.AddButton("Leave", Leave);
                        break;
                    case MemberRankAtGroup.JoinRequest:
                        item.AddButton("Cancel Request", CancelRequest);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //show all groups
            foreach (var group in getAllGroupsTask.Result.Groups)
            {
                //dont show again if exist on my groups
                if (myGroups.Any(x => x.Group.Id == group.Id)) continue;
                //spawn ui item
                var item = Instantiate(groupItemTemplate.gameObject, groupItemTemplate.transform.parent)
                    .GetComponent<GroupsListItem>();
                item.Name = group.Name;
                item.Description = group.Description;
                item.MaxMembersCount = group.MaxCount;
                item.gameObject.SetActive(true);
                _spawnedItems.Add(item);

                async void JoinRequest()
                {
                    await LoginMenu.Client.JoinGroupAsync(LoginMenu.Session, group.Id);
                    Refresh();
                }

                item.AddButton("Join Request", JoinRequest);
            }
        }

        public override void ChangeToThisView()
        {
            base.ChangeToThisView();
            Refresh();
        }
    }
}