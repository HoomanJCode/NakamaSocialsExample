using UnityEngine;
using UnityEngine.UI;

public class GroupsListItem : MonoBehaviour
{
    [SerializeField] private Text rank;
    [SerializeField] private Text nameOfGroup;
    [SerializeField] private Text descOfGroup;
    [SerializeField] private Text maxCount;
    [SerializeField] private Button btn1;

    public MemberRankAtGroup Rank
    {
        set => rank.text = value.ToString();
    }

    public string Name
    {
        set => nameOfGroup.text = value;
    }

    public string Description
    {
        set => descOfGroup.text = value;
    }

    public int MaxMembersCount
    {
        set => maxCount.text = value.ToString();
    }
}