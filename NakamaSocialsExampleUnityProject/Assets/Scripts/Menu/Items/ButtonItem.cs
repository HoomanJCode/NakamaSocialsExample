using System;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Items
{
    public class ButtonItem : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Button button;

        public string Text
        {
            set => text.text = value;
        }

        public Action WhatToDo { get; set; }

        private void Awake()
        {
            button.onClick.AddListener(() => WhatToDo());
        }
    }
}