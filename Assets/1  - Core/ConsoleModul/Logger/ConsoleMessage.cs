using System;
using UnityEngine;

namespace ConsoleModul.Logger
{
    public class ConsoleMessage
    {
        public Color Color { get; private set; } = Color.black;
        public string Message { get; private set;}
        public bool IsBold { get; private set;}

        public ConsoleMessage SetColor(Color color) => BaseRequest(() => Color = color);

        public ConsoleMessage SetMessage(string mes) => BaseRequest(() => Message = mes);

        public ConsoleMessage SetBolt(bool isBolt) => BaseRequest(() => IsBold = isBolt);

        private ConsoleMessage BaseRequest(Action callback)
        {
            callback.Invoke();
            return this;
        }
    }
}