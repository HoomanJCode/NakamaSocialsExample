using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using MenuViews;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class GroupMembersListMenu : MenuView
    {
        private static GroupMembersListMenu _instance;

        private static IEnumerator _enumerator;
        private static string _groupId;
        private static MemberRankAtGroup _rank;
        [SerializeField] private Button refreshBtn;
        [SerializeField] private GroupMembersListItem memberItemTemplate;
        private readonly List<GroupMembersListItem> _spawnedMembers = new();

        public static void SetRequirements(IApiGroup group, MemberRankAtGroup rank)
        {
            _groupId = group.Id;
            _rank = rank;
            _instance.Refresh();
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
            //request members of group
            var membersRequest = LoginMenu.Client.ListGroupUsersAsync(LoginMenu.Session, _groupId, null, 20, "");
            //wait
            yield return new WaitUntil(() => membersRequest.IsCompleted);
            //remove last spawned items
            foreach (var member in _spawnedMembers) Destroy(member.gameObject);
            _spawnedMembers.Clear();
            //spawn members ui
            foreach (var groupUser in membersRequest.Result.GroupUsers)
            {
                var item = Instantiate(memberItemTemplate.gameObject, memberItemTemplate.transform.parent)
                    .GetComponent<GroupMembersListItem>();
                item.Rank = (MemberRankAtGroup) groupUser.State;
                item.UserName = groupUser.User.Username;
                item.gameObject.SetActive(true);
                _spawnedMembers.Add(item);

                async void Accept()
                {
                    await LoginMenu.Client.AddGroupUsersAsync(LoginMenu.Session, _groupId, new[] {groupUser.User.Id});
                    Refresh();
                }

                async void Kick()
                {
                    await LoginMenu.Client.KickGroupUsersAsync(LoginMenu.Session, _groupId,
                        new[] {groupUser.User.Id});
                    Refresh();
                }

                async void Promote()
                {
                    await LoginMenu.Client.PromoteGroupUsersAsync(LoginMenu.Session, _groupId,
                        new[] {groupUser.User.Id});
                    Refresh();
                }

                async void Demote()
                {
                    await LoginMenu.Client.DemoteGroupUsersAsync(LoginMenu.Session, _groupId,
                        new[] {groupUser.User.Id});
                    Refresh();
                }

                async void Ban()
                {
                    await LoginMenu.Client.BanGroupUsersAsync(LoginMenu.Session, _groupId,
                        new[] {groupUser.User.Id});
                    Refresh();
                }

                switch ((MemberRankAtGroup) groupUser.State)
                {
                    case MemberRankAtGroup.SuperAdmin:
                        break;
                    case MemberRankAtGroup.Admin:
                        if (groupUser.User.Id != LoginMenu.Session.UserId && _rank is MemberRankAtGroup.SuperAdmin)
                        {
                            item.AddButton("Promote", Promote);
                            item.AddButton("Demote", Demote);
                            item.AddButton("Kick", Kick);
                            item.AddButton("Ban", Ban);
                        }

                        break;
                    case MemberRankAtGroup.Member:
                        if (_rank is MemberRankAtGroup.Admin or MemberRankAtGroup.SuperAdmin)
                        {
                            item.AddButton("Promote", Promote);
                            item.AddButton("Demote", Demote);
                            item.AddButton("Kick", Kick);
                            item.AddButton("Ban", Ban);
                        }

                        break;
                    case MemberRankAtGroup.JoinRequest:
                        if (_rank is MemberRankAtGroup.Admin or MemberRankAtGroup.SuperAdmin)
                        {
                            item.AddButton("Accept", Accept);
                            item.AddButton("Deny", Kick);
                            item.AddButton("Ban", Ban);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override void Init()
        {
            _instance = this;
            refreshBtn.onClick.AddListener(Refresh);
            memberItemTemplate.gameObject.SetActive(false);
        }
    }
}