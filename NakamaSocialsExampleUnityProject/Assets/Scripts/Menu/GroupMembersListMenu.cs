using System;
using System.Collections;
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

        public static void SetRequirements(IApiGroup group, MemberRankAtGroup rank)
        {
            _groupId = group.Id;
            _rank = rank;
            _instance.Refresh();
        }

        private void Refresh()
        {
            if (_enumerator == null)
                EnumeratorRunner.Singletone.StopCoroutine(_enumerator);
            _enumerator = RefreshEnumerator();
            EnumeratorRunner.Singletone.StartCoroutine(_enumerator);
        }

        private IEnumerator RefreshEnumerator()
        {
            //request members of group
            var membersRequest = Connection.Client.ListGroupUsersAsync(Connection.Session, _groupId, null, 20, "");
            //wait
            yield return membersRequest;
            //spawn members ui
            foreach (var groupUser in membersRequest.Result.GroupUsers)
            {
                var item = Instantiate(memberItemTemplate.gameObject, memberItemTemplate.transform.parent)
                    .GetComponent<GroupMembersListItem>();
                item.Rank = (MemberRankAtGroup) groupUser.State;
                item.UserName = groupUser.User.Username;

                async void Accept()
                {
                    await Connection.Client.AddGroupUsersAsync(Connection.Session, _groupId, new[] {groupUser.User.Id});
                }

                async void Kick()
                {
                    await Connection.Client.KickGroupUsersAsync(Connection.Session, _groupId,
                        new[] {groupUser.User.Id});
                }

                async void Promote()
                {
                    await Connection.Client.PromoteGroupUsersAsync(Connection.Session, _groupId,
                        new[] {groupUser.User.Id});
                }

                async void Demote()
                {
                    await Connection.Client.DemoteGroupUsersAsync(Connection.Session, _groupId,
                        new[] {groupUser.User.Id});
                }

                async void Ban()
                {
                    await Connection.Client.BanGroupUsersAsync(Connection.Session, _groupId,
                        new[] {groupUser.User.Id});
                }

                switch ((MemberRankAtGroup) groupUser.State)
                {
                    case MemberRankAtGroup.SuperAdmin:
                        break;
                    case MemberRankAtGroup.Admin:
                        if (_rank is MemberRankAtGroup.SuperAdmin)
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