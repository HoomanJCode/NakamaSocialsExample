using System;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Items
{
    public class GroupsListItem : MonoBehaviour
    {
        [SerializeField] private Text rank;
        [SerializeField] private Text nameOfGroup;
        [SerializeField] private Text descOfGroup;
        [SerializeField] private Text maxCount;
        [SerializeField] private Toggle isPrivate;
        [SerializeField] private ButtonItem buttonTemplate;

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

        public bool IsPrivate
        {
            set => isPrivate.isOn = value;
        }

        private void Awake()
        {
            buttonTemplate.gameObject.SetActive(false);
        }

        public void AddButton(string text, Action onClick)
        {
            var btn = Instantiate(buttonTemplate.gameObject, buttonTemplate.transform.parent)
                .GetComponent<ButtonItem>();
            btn.gameObject.SetActive(true);
            btn.Text = text;
            btn.WhatToDo = onClick;
        }
    }
}