using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using ConsoleModul.Logger;
using Jint;
using Jint.Native;
using ModelCore.Universal;
using UnityEngine;
using UnityEngine.Events;
using ILogger = ConsoleModul.Logger.ILogger;
using Object = System.Object;

namespace ConsoleModul.PartConsole
{
    public class JsHellper
    {
        public static void DefaultCommand(Engine engine, ILogger loger)
        {
            engine.SetValue("Help", new Action(()=>
            {
                if (loger == null) 
                    return;
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Log(str):void - лог в js консоль"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("StackTrace():void - log last error stack trace"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("FindObject(str):GameObject - найти объект на сцене"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Destroy(Unity.Object):void - удалить объект сцены"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Exit():void - Выход"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Help():void - вывод всех команд"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("HelpCharp(object, isStatic, isDeclared, log:(M | F | P | E | ALl)):void - вывод всех возможных действий с объектом"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Scope(bool):void - показать все переменные в контексте"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("LookAt():GameObject - возврщает объект, на который смотрит игрок"));
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Doc(JsValue, isAllPrototype):void - вывод информации об объекте JS"));
            }));
            engine.SetValue("Log", new Action<object>(x => loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(x.ToString()))));
            
            engine.SetValue("FindObject", new Func<string, GameObject>(x => GameObject.Find(x)));
            engine.SetValue("Destroy", new Action<UnityEngine.Object>(x => GameObject.Destroy(x)));
            engine.SetValue("Exit", new Action(() => Application.Quit()));
            engine.SetValue("HelpCharp", new Action<object, bool, bool, string>((o, isStatic, isDeclared, cfgLog) =>
            {
                string meghodsResult = "";
                string fieldResult="";
                string propResult = "";
                string eventInfo = "";
                GetInfoAboutMFP(o, isStatic, isDeclared, out meghodsResult, out fieldResult, out propResult, out eventInfo);
                if (cfgLog == "M" || cfgLog == "All")
                {
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("Методы"));
                    loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(meghodsResult));
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("____________"));
                }
                if (cfgLog == "F" || cfgLog == "All")
                {
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("Поля"));
                    loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(fieldResult));
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("____________"));
                }
                if (cfgLog == "P" || cfgLog == "All")
                {
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("Проперти"));
                    loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(propResult));
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("____________"));
                }
                if (cfgLog == "E" || cfgLog == "All")
                {
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("События"));
                    loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(eventInfo));
                    loger.Log(new ConsoleMessage().SetColor(Color.yellow).SetMessage("____________"));
                }
            }));
            engine.SetValue("Scope", new Action<bool>(x =>
            {
                loger.Log(new ConsoleMessage().SetColor(Color.green).SetMessage("Существубщий контекст"));
                loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(JsEn.GetScope(engine)));
            }));
            engine.SetValue("Doc", new Action<JsValue, bool>((x, isLoop) =>
            {
                if (!isLoop)
                {
                    loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(JsEn.DocObject(x.AsObject())));
                }
                else
                {
                    var o = x.AsObject();
                    do
                    {
                        loger.Log(new ConsoleMessage().SetColor(Color.white).SetMessage(JsEn.DocObject(o)));
                        o = o.Prototype;
                    } while (o.Prototype != null);
                }
            }));
        }

        private static void GetInfoAboutMFP(object o, bool isStatic, bool isDesclaredOnly, out string meghodsResult, out string fieldResult, out string propResult, out string eventsResult)
        {
            BindingFlags flags = BindingFlags.Public;
            if (isStatic)
                flags = flags | BindingFlags.Static;
            else
                flags = flags | BindingFlags.Instance;
            if (isDesclaredOnly)
                flags = flags | BindingFlags.DeclaredOnly;
            MethodInfo[] methods = null;
            FieldInfo[] fields = null;
            PropertyInfo[] prop = null;
            EventInfo[] events = null;
            if (o is Type)
            {
                var oT = o as Type;

                events = oT.GetEvents(flags);
                methods = oT.GetMethods(flags);
                fields = oT.GetFields(flags);
                prop = oT.GetProperties(flags);
            }
            else
            {
                events = o.GetType().GetEvents(flags);
                methods = o.GetType().GetMethods(flags);
                fields = o.GetType().GetFields(flags);
                prop = o.GetType().GetProperties(flags);
            }

            meghodsResult = "";
            foreach (var info in methods)
            {
                var param = info.GetParameters();
                string strParam = "";
                foreach (var parameterInfo in param)
                {
                    strParam += parameterInfo.ParameterType.ToString() + " ,  ";
                }
                meghodsResult += $"{info.Name}({strParam})\n";
            }

            fieldResult = "";
            foreach (var f in fields) fieldResult += $"{f.Name}; ({f.FieldType}) / \n";

            propResult = "";
            foreach (var p in prop) propResult += $"{p.Name}; (R:{p.CanRead}; W:{p.CanWrite}; Type:{p.PropertyType})\n";

            eventsResult = "";
            foreach (var info in events) eventsResult += $"{info.Name}; Raise:({info.RaiseMethod}; Handler type: {info.EventHandlerType}\n";
        }

    }
}