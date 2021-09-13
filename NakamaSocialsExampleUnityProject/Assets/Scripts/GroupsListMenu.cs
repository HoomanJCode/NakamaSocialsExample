using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using MenuViews;
using UnityEngine;
using UnityEngine.UI;

public class GroupsListMenu : MenuView
{
    [SerializeField] private InputField searchBox;
    [SerializeField] private GroupsListItem groupItemTemplate;
    private readonly List<GroupsListItem> _spawnedItems = new();

    private IEnumerator _refreshEnumerator;

    protected override void Init()
    {
        searchBox.onValueChanged.AddListener(_ => Refresh());
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
        foreach (var group in getMyGroupsTask.Result.UserGroups)
        {
            //spawn ui item
            var item = Instantiate(groupItemTemplate.gameObject, groupItemTemplate.transform.parent)
                .GetComponent<GroupsListItem>();
            item.Name = group.Group.Name;
            item.Description = group.Group.Description;
            item.MaxMembersCount = group.Group.MaxCount;
            item.Rank = (MemberRankAtGroup) group.State;
            _spawnedItems.Add(item);
        }

        //show all groups
        foreach (var group in getAllGroupsTask.Result.Groups)
        {
            //dont show again if exist on my groups
            if (getMyGroupsTask.Result.UserGroups.Any(x => x.Group.Id == group.Id)) continue;
            //spawn ui item
            var item = Instantiate(groupItemTemplate.gameObject, groupItemTemplate.transform.parent)
                .GetComponent<GroupsListItem>();
            item.Name = group.Name;
            item.Description = group.Description;
            item.MaxMembersCount = group.MaxCount;
            _spawnedItems.Add(item);
        }
    }

    public override void ChangeToThisView()
    {
        base.ChangeToThisView();
        Refresh();
    }
}