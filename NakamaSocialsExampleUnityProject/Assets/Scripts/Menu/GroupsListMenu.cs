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
        private readonly List<GroupsListItem> _spawnedItems = new();

        private IEnumerator _refreshEnumerator;

        protected override void Init()
        {
            searchBox.onValueChanged.AddListener(_ => Refresh());
            groupItemTemplate.gameObject.SetActive(false);
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
            var getMyGroupsTask = Connection.Client.ListUserGroupsAsync(Connection.Session, null, 20, "");
            //get other exist groups by search text
            var getAllGroupsTask = Connection.Client.ListGroupsAsync(Connection.Session, $"{searchBox.text}%", 20);
            //wait
            yield return getMyGroupsTask;
            yield return getAllGroupsTask;
            //clear list
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
                item.Rank = (MemberRankAtGroup) receivedGroupData.State;
                item.gameObject.SetActive(true);
                _spawnedItems.Add(item);

                async void Leave()
                {
                    await Connection.Client.LeaveGroupAsync(Connection.Session, receivedGroupData.Group.Id);
                }

                void Members()
                {
                    GroupMembersListMenu.SetRequirements(receivedGroupData.Group,
                        (MemberRankAtGroup) receivedGroupData.State);
                    ChangeCurrentView<GroupMembersListMenu>();
                }

                async void CancelRequest()
                {
                    await Connection.Client.LeaveGroupAsync(Connection.Session, receivedGroupData.Group.Id);
                }

                void UpdateGroup()
                {
                    UpdateGroupMenu.SetRequirements(receivedGroupData.Group);
                    ChangeCurrentView<UpdateGroupMenu>();
                }

                async void Remove()
                {
                    await Connection.Client.DeleteGroupAsync(Connection.Session, receivedGroupData.Group.Id);
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
            }
        }

        public override void ChangeToThisView()
        {
            base.ChangeToThisView();
            Refresh();
        }
    }
}