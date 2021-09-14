using System;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Items
{
    public class FriendsListItem : MonoBehaviour
    {
        [SerializeField] private Text userName;
        [SerializeField] private ButtonItem buttonTemplate;

        public string UserName
        {
            set => userName.text = value;
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