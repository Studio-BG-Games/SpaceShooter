using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Codice.CM.Common.Serialization;
using CorePresenter.UniversalPart;
using DIContainer;
using JetBrains.Annotations;
using ModelCore;
using ModelCore.Universal;
using ModelCore.Universal.AliasValue;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Toggle = UnityEngine.UIElements.Toggle;


public class RootEditor : EditorWindow
{
    private static string PathToRootWin = "Assets/1  - Core/MVP/Editor/WindowEditRoot.uxml";
    private static Dictionary<Type, string> ModelAndType = new Dictionary<Type, string>()
    {
        {typeof(AliasBool), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/BoolView.uxml"},
        {typeof(AliasFloat), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/FloatView.uxml"},
        {typeof(AliasInt), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/IntView.uxml"},
        {typeof(RootModel), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/RootView.uxml"},
        {typeof(AliasString), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/StringView.uxml"},
        {typeof(AliasVector2), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/V2View.uxml"},
        {typeof(AliasVector3), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/V3View.uxml"},
        {typeof(JsEvent), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/EventView.uxml"},
        {typeof(IJsEventT), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/EventView.uxml"},
        {typeof(JsEn), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/JsEn.uxml"},
        {typeof(CsEn), "Assets/1  - Core/MVP/Editor/UXML/ModelViews/CsEn.uxml"}
    };

    public event Action Refresh;
    public event Action<string> ComandForView; // HideAll

    private static ObjectField _fileField;
    private VisualElement _panelCreateButtons;
    
    public RootModel TargetRoot;
    private ScrollView _scrollRoot;
    private ViewRoot _viewRoot;
    public ViewRoot SelectedRoot { get; private set; }

    private string _pathToRootFolder;
    private TextField _pathField;
    private TextField _nameFileField;

    private Type[] TypesModels => _typesModels ??= typeof(Model).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Model)) && x.IsAbstract == false).ToArray();
    private Type[] _typesModels;

    [InitializeOnLoadMethod]
    private static void SubscribeToDebug()
    {
        P_DebugModel.DebugAction += x => ShowWindow(x);
    }
    
    [MenuItem("MVP/RootEditor")]
    public static void ShowWindow()
    {
        var win = GetWindow<RootEditor>("RootEditor");
        win.Show();
    }

    public static void ShowWindow(RootModel root)
    {
        var win = GetWindow<RootEditor>("RootEditor");
        win.Show();
        win.SetNewRoot(root);
    }

    public void CreateGUI()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathToRootWin);
        var child = tree.Instantiate();
        rootVisualElement.Add(child);
        
        // Save and Load
        _pathToRootFolder = rootVisualElement.Q<TextField>("Path").text;
        _pathField = rootVisualElement.Q<TextField>("Path");
        _nameFileField = rootVisualElement.Q<TextField>("NameFile");
        _pathField.RegisterCallback<BlurEvent>(x=> _pathToRootFolder = rootVisualElement.Q<TextField>("Path").text);
        rootVisualElement.Q<Button>("SaveByPath").clickable.clicked += () =>
        {
            var fullpath = Path.Combine(Application.dataPath, _pathField.text);
            if (!Directory.Exists(fullpath)) Directory.CreateDirectory(fullpath);
            var pathToFile = Path.Combine(fullpath, _nameFileField.text + ".json");
            File.WriteAllText(pathToFile, TargetRoot.Save());
            SendMessageForUser($"Файл сохранен по пути: {pathToFile}", Color.green);
            AssetDatabase.Refresh();
        };
        
        
        // hide and show
        rootVisualElement.Q<Button>("ShowHideCreate").clickable.clicked += () => HideShow(rootVisualElement.Q("CreateButtons"));
        rootVisualElement.Q<Button>("Btn_ShowHideTemplate").clickable.clicked += () => HideShow(rootVisualElement.Q("PanelTemplates"));
        rootVisualElement.Q<Button>("btn_Create").clickable.clicked += () => CreateMain(rootVisualElement.Q<TextField>("S_NameForNew").text);
        rootVisualElement.Q<Button>("Refresh").clickable.clicked += () =>
        {
            SetNewRoot(TargetRoot);
            Refresh?.Invoke();
        };

        // Work with file
        var btnSave = rootVisualElement.Q<Button>("Save");
        var boolBtnSave = rootVisualElement.Q<Toggle>("EnableSave");
        btnSave.SetEnabled(boolBtnSave.value);
        boolBtnSave.RegisterCallback<ChangeEvent<bool>>(x=>btnSave.SetEnabled(x.newValue));
        btnSave.clickable.clicked += ()=>
        {
            Save();
            boolBtnSave.value = false;
        };
        rootVisualElement.Q<Button>("Load").clickable.clicked += Load;
        rootVisualElement.Q<Button>("Btn_HideAll").clickable.clicked +=()=> ComandForView?.Invoke("HideAll");
        _fileField = rootVisualElement.Q<ObjectField>("JsonFile");
        rootVisualElement.Q<ObjectField>("JsonFile").RegisterCallback<ChangeEvent<Object>>(x =>
        {
            if (x.newValue == null) return;
            var path = AssetDatabase.GetAssetPath(x.newValue);
            if (!path.Contains(".json") && !path.Contains(".txt")) rootVisualElement.Q<ObjectField>("JsonFile").value = null;
        });

        
        // Create buttons
        _panelCreateButtons = rootVisualElement.Q<VisualElement>("CreateButtons");
        GenerateButtons(_panelCreateButtons);
        
        //Create First Root
        _scrollRoot = rootVisualElement.Q<ScrollView>("ViewRoot");
        _scrollRoot.RegisterCallback<MouseDownEvent>(x => { if(x.button==2 && SelectedRoot!=null) SelectedRoot.ShowHide(); });
        if(TargetRoot==null) CreateMain("Main");

        //Init template panel
        new ViewTemplates(this, rootVisualElement.Q<ScrollView>("PanelTemplates"), rootVisualElement.Q<Button>("Btn_CreateTemplate"));
        SendMessageForUser("Hello =]", Color.white);
    }

    private TextAsset GetFile() => rootVisualElement.Q<ObjectField>("JsonFile").value as TextAsset;

    private void CreateMain(string name) => SetNewRoot(new RootModel(name));

    private void SetNewRoot(RootModel model)
    {
        _scrollRoot.contentContainer.hierarchy.Clear();
        TargetRoot = model;
        _viewRoot = new ViewRoot(TargetRoot, _scrollRoot, this);
        _viewRoot.SetActiveContent(true);
        Select(_viewRoot);
    }

    private void GenerateButtons(VisualElement panelCreateButtons)
    {
        TypesModels.ForEach(x =>
        {
            var color = GetColorByModel(x);
            if(color==DefaultColor) return;
            
            var button = new Button(()=>
            {
                if (SelectedRoot == null) return;
                Model newModel = null;
                if (x.GetInterfaces().Contains(typeof(IAliasValue)))
                    newModel = (Model) Activator.CreateInstance(x, new object[] {$"Rnd={Random.Range(-10000, 10001)}", default});
                else if (x  == typeof(CsEn))
                {
                    SearchCsModelScript.Show(z =>
                    {
                        var scriptInstance = Activator.CreateInstance(z);
                        if (scriptInstance == null)
                        {
                            Debug.LogWarning("Не удалось создать CsEn с типом - "+z.Name);
                            return;
                        }
                        SelectedRoot.GetModel.AddModel(new CsEn((CsEn.BaseScript)scriptInstance));
                    });
                }
                else
                    newModel = (Model) Activator.CreateInstance(x, new object[] {$"Rnd={Random.Range(-10000, 10001)}"});
                SelectedRoot.GetModel.AddModel(newModel);
            });
            
            button.text = "Create " + x.Name;
            button.style.backgroundColor = new StyleColor(color);
            
            panelCreateButtons.Add(button);
        });
    }

    private Color DefaultColor = new Color(0.25f, 0.25f, 0.25f);

    private Color GetColorByModel(Type type)
    {
        if(type == typeof(RootModel)) return new Color(0.92f, 0.3f, 0.42f);
        if(type.GetInterfaces().Contains(typeof(IAliasValue))) return new Color(0.65f, 0.82f, 0.8f);
        if(type == typeof(JsEvent) || type.GetInterfaces().Contains(typeof(IJsEventT))) return new Color(0.9f, 0.62f, 0.92f);
        if(type== typeof(JsEn) || type ==typeof(CsEn)) return new Color(0.64f, 0.9f, 0.6f);
        return DefaultColor;
    }

    private void Load()
    {
        var file = GetFile();
        if(file==null)
        {
            SendMessageForUser("Не могу загрузить, нет выбранного файла", Color.yellow);
            return;
        }
        var model = JsonConvert.DeserializeObject<RootModel>(file.text, RootModel.Factory.SettingJson());
        if (model == null || string.IsNullOrWhiteSpace(model.Alias))
        {
            SendMessageForUser("Из файла нельзя загрузить RootModel", Color.red);
            return;
        }
        SetNewRoot(model);
        SendMessageForUser("Загружен", Color.green);
    }

    private void Save()
    {
        var file = GetFile();
        if (file == null)
        {
            SendMessageForUser("Не могу сохранить, нет выбранного файла", Color.yellow);
            return;
        }

        var path = AssetDatabase.GetAssetPath(file);
        File.WriteAllText(path, TargetRoot.Save());
        EditorUtility.SetDirty(file);
        AssetDatabase.Refresh();
        SendMessageForUser("Сохранен", Color.green);
    }

    private void SendMessageForUser(string name) => SendMessageForUser(name, Color.white);

    private void SendMessageForUser(string name, Color c)
    {
        rootVisualElement.Q<Label>("Message").text = name;
        rootVisualElement.Q<Label>("Message").style.color = c;
    }

    public void Select(ViewRoot viewRoot)
    {
        SelectedRoot?.Deselect();
        SelectedRoot = viewRoot;
        SelectedRoot.Select();
    }

    public static void HideShow(VisualElement visualElement)
    {
        if(visualElement.style.display == DisplayStyle.None) visualElement.style.display = DisplayStyle.Flex;
        else visualElement.style.display = DisplayStyle.None;
    }

    public static void SetShow(VisualElement elelemtn, bool active)
    {
        if (active) elelemtn.style.display = DisplayStyle.Flex;
        else elelemtn.style.display = DisplayStyle.None;
    }

    public class ViewTemplates
    {
        private static string PathTotemplatesFiles = Path.Combine(Application.dataPath, PathToTemplateFileReletive);
        private static string PathToTemplateFileReletive => "Editor/Data for editor root window/Templates";
        private static string PathToTemplateView => "Assets/1  - Core/MVP/Editor/UXML/ModelViews/Parts/TemplateView.uxml";
        private static TextAsset[] Templates;
        private RootEditor _window;
        private ScrollView _scroll;

        [InitializeOnLoadMethod]
        public static void UpdateTemplates()
        {
            if (!Directory.Exists(PathTotemplatesFiles)) Directory.CreateDirectory(PathTotemplatesFiles);
            var paths = AssetDatabase.FindAssets("t:TextAsset", new[] {"Assets/" + PathToTemplateFileReletive});
            List<TextAsset> step1 = new List<TextAsset>();
            paths.ForEach(x => step1.Add(AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(x))));
            Templates = step1.Where(x=>AssetDatabase.GetAssetPath(x).Contains(".json")).ToArray();
        }

        public ViewTemplates(RootEditor window, ScrollView scroll, Button createBtn)
        {
            _window = window;
            _scroll = scroll;
            _window.Refresh +=()=> Refresh();
            createBtn.clickable.clicked +=()=>
            {
                SaveSelected();
                Refresh();
            };
            Refresh();
        }

        public void SaveSelected()
        {
            var model = _window.SelectedRoot.GetModel;
            if (model == null)
            {
                _window.SendMessageForUser("Не выбран root для сохранения в шаблон", Color.red);
                return;
            }
            string nameForSave = model.Alias;

            if (!CanSave(nameForSave))
            {
                _window.SendMessageForUser($"{nameForSave} - уже существует, выберите иное имя", Color.red);
                return;
            }
            
            File.WriteAllText(PathTotemplatesFiles+"/"+nameForSave+".json", model.Save());
            _window.SendMessageForUser($"Шаблон с именем {nameForSave} сохранен по пути {PathTotemplatesFiles}", Color.green);
            AssetDatabase.Refresh();

            bool CanSave(string name)
            {
                var path = Path.Combine(PathTotemplatesFiles, $"{name}.json");
                return !File.Exists(path);
            }
        }

        private void SpawnTemplates()
        {
            _scroll.contentContainer.hierarchy.Clear();
            var treeTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathToTemplateView);
            Templates.ForEach(x =>
            {
                var view = treeTemplate.Instantiate();
                _scroll.contentContainer.hierarchy.Add(view);
                new View(x, view, this);
            });
        }

        private void Refresh()
        {
            UpdateTemplates();
            SpawnTemplates();
        }

        private class View
        {
            public View(TextAsset asset, VisualElement view, ViewTemplates parent)
            {
                //NameLabel Spawn Delete
                view.Q<Label>("NameLabel").text = asset.name;
                view.Q<Button>("Spawn").clickable.clicked += () =>
                {
                    var model = JsonConvert.DeserializeObject<RootModel>(asset.text, RootModel.Factory.SettingJson());
                    if (model == null)
                    {
                        parent._window.SendMessageForUser($"Шаблон {asset.name} битый, ничего не загружено", Color.yellow);
                        return;
                    }
                    model.Rename(model.Alias + "-" + Random.Range(0, 10000).ToString());
                    parent._window.SelectedRoot.GetModel.AddModel(model);
                    parent._window.SendMessageForUser($"Шаблон {asset.name} успешно загружен", Color.green);
                };
                view.Q<Button>("Delete").clickable.clicked += () =>
                {
                    parent._window.SendMessageForUser($"Шаблон {asset.name} удален");
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                    parent.Refresh();
                };
            }
        }
    }

    public abstract class ViewModelCore<T> where T : Model
    {
        protected T Model;
        public T GetModel => Model;
        protected VisualElement ViewOfModel;
        protected VisualElement Parent;
        public RootEditor EditWindow { get; private set; }
        
        public object CreateAliasValueView(IAliasValue model, VisualElement parent, RootEditor windowEditor) 
        {
            
            if (model.TypeOfValue == typeof(int)) return new ViewInt(model as AliasInt, parent, windowEditor);
            if (model.TypeOfValue == typeof(float)) return new ViewFloat(model as AliasFloat, parent, windowEditor);
            if (model.TypeOfValue == typeof(string)) return new ViewString(model as AliasString, parent, windowEditor);
            if (model.TypeOfValue == typeof(bool)) return new ViewBool(model as AliasBool, parent, windowEditor);
            if (model.TypeOfValue == typeof(Vector2)) return new ViewVector2(model as AliasVector2, parent, windowEditor);
            if (model.TypeOfValue == typeof(Vector3)) return new ViewVector3(model as AliasVector3, parent, windowEditor);
            throw new Exception("Неизвестный тип для отображение Alias Value; Тип = "+model.TypeOfValue.Name);
        }

        public object CreateEventView(IJsEventT model, VisualElement parent, RootEditor windowEditor)
        {
            if (model.GetTypeEvent == typeof(RootModel)) return new ViewValueEvent<RootModel>(model as JsEventT1<RootModel>, parent, windowEditor);
            if (model.GetTypeEvent == typeof(string)) return new ViewValueEvent<string>(model as JsEventT1<string>, parent, windowEditor);
            throw new Exception("Неизвестный тип для отображение Alias Event; Тип = "+model.GetTypeEvent.Name);
        }
        
        public ViewModelCore(T model, VisualElement parent, RootEditor windowEditort)
        {
            Model = model;
            EditWindow = windowEditort;
            ViewOfModel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootEditor.ModelAndType[GetTypeForSearchTree()]).Instantiate();
            Parent = parent;
            parent.Add(ViewOfModel);
            BindValue();
        }

        protected virtual Type GetTypeForSearchTree() => typeof(T);

        public virtual void BindValue()
        {
            ViewOfModel.Q<Label>("Prefix").text = Model.Prefics;
            ViewOfModel.Q<Button>("Btn_Delete").clickable.clicked += () =>
            {
                if(Model.Root!=null) Model.Root.DeleteId(Model.IdModel);
            };
            SetFullId();
        }

        protected void SetInfoAliasValue<T2>(BaseAliasValue<T2> modelForEdit) 
        {
            var alias = ViewOfModel.Q<TextField>("Alias");
            var value = ViewOfModel.Q("Value");
            
            alias.value = modelForEdit.Alias;
            value.GetType().GetProperty("value").SetValue(value, modelForEdit.Value);
            
            var prevName = "";
            alias.RegisterCallback<FocusInEvent>(x=>prevName = alias.value);
            alias.RegisterCallback<BlurEvent>(x =>
            {
                if (!modelForEdit.Rename(alias.value)) alias.value = prevName;
                else alias.value = modelForEdit.Alias;
                SetFullId();
            });
            
            value.RegisterCallback<ChangeEvent<T2>>(x=>modelForEdit.Value = x.newValue);
        }
        
        protected void SetFullId()
        {
            var label = ViewOfModel.Q<Label>("FullId");
            label.text = "Full Id = " + Model.IdModel;
        }
        
        protected void BindRanameField(TextField field, string startName)
        {
            field.value = startName;
            string prevName = "";
            field.RegisterCallback<FocusInEvent>(x=>prevName = field.value);
            field.RegisterCallback<BlurEvent>(x =>
            {
                if(Model.Rename(field.value)) field.value = field.value;
                else field.value = prevName;
                SetFullId();
            });
        }
    }

    public class ViewRoot : ViewModelCore<RootModel>, IDisposable
    {
        private VisualElement _infoPanel;
        private TextField _aliasField;
        private VisualElement _container;

        public ViewRoot(RootModel model, VisualElement parent, RootEditor editor) : base(model, parent, editor){}

        public override void BindValue()
        {
            base.BindValue();
            EditWindow.ComandForView += TryHide;
            _infoPanel = ViewOfModel.Q<VisualElement>("Info");
            _container = ViewOfModel.Q<VisualElement>("container");
            
            _aliasField = ViewOfModel.Q<TextField>("Alias");
            _aliasField.value = Model.Alias;

            string prevName = "";
            _aliasField.RegisterCallback<FocusInEvent>(x=>prevName = _aliasField.value);
            _aliasField.RegisterCallback<BlurEvent>(x =>
            {
                if(Model.Rename(_aliasField.value)) SetName(_aliasField.value);
                else SetName(prevName);
            });

            ViewOfModel.Q<Button>("Btn_Select").clickable.clicked += () => EditWindow.Select(this);

            ViewOfModel.Q<Foldout>("ShowHideTog").RegisterCallback<ChangeEvent<bool>>(x => SetActiveContent(x.newValue));
            Deselect();
            
            ViewOfModel.Q<Button>("Btn_Copy").clickable.clicked+= () =>
            {
                if(EditWindow.SelectedRoot.Model!=null)
                    CloneTo(EditWindow.SelectedRoot.Model);
            };
            ViewOfModel.Q<Button>("Btn_CopyParent").clickable.clicked+= () =>
            {
                if(Model.Root!=null)
                    CloneTo(Model.Root);
            };
            
            Model.CountModelsCahnged += UpdateChilds;
            UpdateChilds();
        }

        private void TryHide(string obj)
        {
            if(obj!="HideAll") return;
            SetActiveContent(false);
        }

        private void CloneTo(RootModel otherRoot)
        {
            var newModel = Model.Clone();
            newModel.Rename(Model.Alias + $"-Clone{Random.Range(0, 10000)}");
            otherRoot.AddModel(newModel);
        }

        private void UpdateChilds()
        {
            _container.hierarchy.Clear();
            Model.Slaves.ForEach(x =>
            {
                var t = x.GetType();
                if (x is RootModel)  new ViewRoot(x as RootModel, _container, EditWindow);
                if (x is IAliasValue) CreateAliasValueView(x as IAliasValue, _container, EditWindow);
                if (x is JsEvent) new ViewEvent(x as JsEvent, _container, EditWindow);
                if (x is IJsEventT) CreateEventView(x as IJsEventT, _container, EditWindow);
                if (x is JsEn) new ViewJsEn(x as JsEn, _container, EditWindow);
                if (x is CsEn) new ViewCsEn(x as CsEn, _container, EditWindow);
            });
            SetActiveContent(true);
        }

        public void ShowHide()
        {
            SetActiveContent(_container.style.display == DisplayStyle.None);
        }
        
        public void SetActiveContent(bool x)
        {
            if (x) _container.style.display = DisplayStyle.Flex;
            else _container.style.display = DisplayStyle.None;
        }

        private void SetName(string name)
        {
            _aliasField.value = name;
            SetFullId();
        }

        public void Select()
        {
            ViewOfModel.Q<Button>("Btn_Select").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _infoPanel.style.backgroundColor = new StyleColor(new Color(1, 1, 1));
        }

        public void Deselect()
        {
            ViewOfModel.Q<Button>("Btn_Select").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _infoPanel.style.backgroundColor = new StyleColor(new Color(0.46f, 0.46f, 0.46f, 0));
        }

        public void Dispose() => EditWindow.ComandForView -= TryHide;
    }
    
    public class ViewEvent : ViewModelCore<JsEvent>
    {
        public ViewEvent(JsEvent model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort)
        {
        }

        public override void BindValue()
        {
            base.BindValue();
            BindRanameField(ViewOfModel.Q<TextField>("Alias"), Model.Alias);
            ViewOfModel.Q<Label>("Type").text = "Type - Empty";
        }
    }
    
    public class ViewValueEvent<T> : ViewModelCore<JsEventT1<T>>
    {
        public ViewValueEvent(JsEventT1<T> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort)
        {
        }

        protected override Type GetTypeForSearchTree() => typeof(IJsEventT);

        public override void BindValue()
        {
            base.BindValue();
            BindRanameField(ViewOfModel.Q<TextField>("Alias"), Model.Alias);
            ViewOfModel.Q<Label>("Type").text = $"Type - {Model.GetTypeEvent.Name}";
        }
    }

    public class ViewJsEn : ViewModelCore<JsEn>
    {
        private static List<SearchScriptJsEditor.Data> Scripts;
        private Label _scriptName;

        public ViewJsEn(JsEn model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort)
        {
        }

        [InitializeOnLoadMethod]
        public static void UpdateScripts()
        {
            Scripts = new List<SearchScriptJsEditor.Data>();
            List<string> paths = new List<string>();
            AssetDatabase.FindAssets("t:TextAsset").ForEach(x => paths.Add(AssetDatabase.GUIDToAssetPath(x)));
            paths.Where(x => x.Contains(".js") && !x.Contains(".json")).ForEach(x =>
            {
                var result = AssetDatabase.LoadAssetAtPath<TextAsset>(x);
                if(result!=null) Scripts.Add(new SearchScriptJsEditor.Data(x, result));
            });
        }

        public override void BindValue()
        {
            base.BindValue();
            _scriptName = ViewOfModel.Q<Label>("ScriptName");
            _fileField = ViewOfModel.Q<ObjectField>("ScriptField");

            SetScript( Scripts.FirstOrDefault(x => x.Asset.name == Model.NameScript), true);
            ViewOfModel.Q<Button>("ViewScript").clickable.clicked +=()=> SearchScriptJsEditor.Show(Scripts.ToArray(), x=>SetScript(x));
        }

        private void SetScript(SearchScriptJsEditor.Data obj, bool ignoreRename = false)
        {
            var resultRename = Model.Rename(obj != null ? obj.Asset.name : "");

            if (resultRename || ignoreRename)
            {
                _fileField.value = obj != null ? obj.Asset : null;
                _scriptName.text = obj != null ? obj.Asset.name : "None";
            }
        }
    }
    
    public class ViewCsEn : ViewModelCore<CsEn>
    {
        public ViewCsEn(CsEn model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort)
        {
            
        }

        public override void BindValue()
        {
            base.BindValue();
            ViewOfModel.Q("InfoPanel").RegisterCallback<MouseDownEvent>(x => { if (x.button == 0) RootEditor.SetShow(ViewOfModel.Q("InfoPanel"), false); });
            ViewOfModel.Q<Button>("Btn_I").clickable.clicked += () => RootEditor.HideShow(ViewOfModel.Q("InfoPanel"));
            ViewOfModel.Q<Label>("Prefix").text = Model.IdModel;

            var infoAtr = Model.Script.GetType().GetCustomAttribute<Info>();
            ViewOfModel.Q<Label>("InfoText").text = infoAtr != null ? infoAtr.Value : "None";

            SpawnFields(ViewOfModel.Q<Foldout>("Panel_Fields").contentContainer);
            SpawnMethods(ViewOfModel.Q<Foldout>("Panel_Methods").contentContainer);
        }

        private void SpawnFields(VisualElement contentContainer)
        {
            contentContainer.hierarchy.Clear();
            var elements = GenenarateInputs(Model.Script);
            elements.ForEach(x => contentContainer.Add(x));

            List<VisualElement> GenenarateInputs(object instance)
            {
                var fileds = instance.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.IsPublic || x.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>() != null);
                
                List<VisualElement> result = new List<VisualElement>();
                foreach (var f in fileds)
                {
                    var minWidth = 25;
                    var maxWidth = 135;
                    if (f.FieldType == typeof(int))
                    {
                        var input = new IntegerField(f.Name);
                        input.Q("unity-text-input").style.minWidth = minWidth;
                        input.Q("unity-text-input").style.maxWidth = maxWidth;
                        result.Add(input);
                        input.value = (int)f.GetValue(instance);
                        RegisterCallbackInput<int>(input, f);
                    }
                    else if (f.FieldType == typeof(float))
                    {
                        var input = new FloatField(f.Name);
                        input.Q("unity-text-input").style.minWidth = minWidth;
                        input.Q("unity-text-input").style.maxWidth = maxWidth;
                        result.Add(input);
                        input.value = (float)f.GetValue(instance);
                        RegisterCallbackInput<float>(input, f);
                    }
                    else if (f.FieldType == typeof(string))
                    {
                        var input = new TextField(f.Name);
                        input.multiline = true;
                        input.Q("unity-text-input").style.minWidth = minWidth;
                        input.Q("unity-text-input").style.maxWidth = maxWidth;
                        result.Add(input);
                        input.value = (string)f.GetValue(instance);
                        RegisterCallbackInput<string>(input, f);
                    }
                    else if (f.FieldType == typeof(bool))
                    {
                        var input = new Toggle(f.Name);
                        result.Add(input);
                        input.value = (bool)f.GetValue(instance);
                        RegisterCallbackInput<bool>(input, f);
                    }
                    else if (f.FieldType.IsEnum)
                    {
                        var input = new EnumField(f.Name, (Enum)f.GetValue(instance));
                        result.Add(input);
                        RegisterCallbackInput<Enum>(input, f);
                    }
                    else if(f.FieldType.IsSubclassOf(typeof(ValueType)))
                    {
                        if (f.FieldType == typeof(Vector2))
                        {
                            var input = new Vector2Field(f.Name);
                            result.Add(input);
                            input.value = (Vector2)f.GetValue(instance);
                            RegisterCallbackInput<Vector2>(input, f);
                        }
                        else if (f.FieldType == typeof(Vector3))
                        {
                            var input = new Vector3Field(f.Name);
                            result.Add(input);
                            input.value = (Vector3)f.GetValue(instance);
                            RegisterCallbackInput<Vector3>(input, f);
                        }
                        else
                        {
                            var foldaout = new Foldout();
                            foldaout.text = f.Name;
                            GenenarateInputs(f.GetValue(instance)).ForEach(x=>foldaout.contentContainer.Add(x));
                            result.Add(foldaout);
                        }
                    }
                }

                return result;
                void RegisterCallbackInput<T>(BaseField<T> input, FieldInfo info)
                {
                    input.RegisterCallback<ChangeEvent<T>>(x => info.SetValue(instance, x.newValue));
                    Model.Script.Valid();
                }
            }
        }

        private void SpawnMethods(VisualElement contentContainer)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var baseMethods = typeof(CsEn.BaseScript).GetMethods(flags).Select(x => x.Name).ToList();
            var methods = Model.Script.GetType().GetMethods(flags).Where(x=>!baseMethods.Contains(x.Name)).ToArray();
            
            contentContainer.Clear();
            methods.ForEach(x =>
            {
                var label = new Label();
                label.style.fontSize = 15;
                label.style.paddingTop = label.style.paddingBottom = 3;
                label.style.paddingLeft = label.style.paddingRight = 0;
                label.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
                var str = $"{x.Name} ({GetArguments(x)}) : {x.ReturnType}";
                label.text = str;
                contentContainer.Add(label);
            });

            string GetArguments(MethodInfo m)
            {
                string result = "";
                var param = m.GetParameters();
                for (var i = 0; i < param.Length; i++)
                {
                    result += $"{param[i].ParameterType.Name} {param[i].Name}";
                    if (i != param.Length - 1) result += ", ";
                }

                return result;
            }
        }
    }
    
    #region ViewAliasValue
    public abstract class ViewBaseAliasValue<T> : ViewModelCore<BaseAliasValue<T>>
    {
        public ViewBaseAliasValue(BaseAliasValue<T> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort)
        {
        }

        protected override Type GetTypeForSearchTree()
        {
            if (typeof(T) == typeof(int)) return typeof(AliasInt);
            if (typeof(T) == typeof(float)) return typeof(AliasFloat);
            if (typeof(T) == typeof(string)) return typeof(AliasString);
            if (typeof(T) == typeof(bool)) return typeof(AliasBool);
            if (typeof(T) == typeof(Vector2)) return typeof(AliasVector2);
            if (typeof(T) == typeof(Vector3)) return typeof(AliasVector3);
            throw new Exception("Неизвестный тип для отображение Alias Value; Тип = "+typeof(T).Name);
        }
        
        public override void BindValue()
        {
            base.BindValue();
            SetInfoAliasValue(Model);
        }
    }
     
    public class ViewInt : ViewBaseAliasValue<int> {
        public ViewInt(BaseAliasValue<int> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort) { }
    }
        
    public class ViewFloat : ViewBaseAliasValue<float> {
        public ViewFloat(BaseAliasValue<float> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort) { }
    }
        
    public class ViewString : ViewBaseAliasValue<string> {
        public ViewString(BaseAliasValue<string> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort) { }
    }
        
    public class ViewVector2 : ViewBaseAliasValue<Vector2> {
        public ViewVector2(BaseAliasValue<Vector2> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort) { }
    }
        
    public class ViewVector3 : ViewBaseAliasValue<Vector3> {
        public ViewVector3(BaseAliasValue<Vector3> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort) { }
    }
        
    public class ViewBool : ViewBaseAliasValue<bool> {
        public ViewBool(BaseAliasValue<bool> model, VisualElement parent, RootEditor windowEditort) : base(model, parent, windowEditort) { }
    }
    #endregion
}

public class SearchScriptJsEditor : EditorWindow
{
    private Data[] _datas;
    private string PathToWindow = "Assets/1  - Core/MVP/Editor/UXML/WindowSearchJsScripts.uxml";
    private ScrollView _scrollScript;
    private Action<Data> _callback;

    private void Init(Data[] datas, Action<Data> callbcakOnSelected)
    {
        _datas = datas;
        _callback = callbcakOnSelected;
    }

    [MenuItem("MVP/new JS", false, 10)]
    public static void CreateJSScript()
    {
        var path = "Assets/1 - Scripts/Js/";

        var pathFile = Path.Combine(path, $"Js-{Random.Range(0, 100)}.js");
        if(File.Exists(pathFile)) return;
        
        File.WriteAllText(pathFile, "");
        AssetDatabase.Refresh();
    }

    public static void Show(Data[] datas, Action<Data> callbcakOnSelected)
    {
        var win = CreateInstance<SearchScriptJsEditor>();
        win.Init(datas, callbcakOnSelected);
        win.ShowAuxWindow();
    }

    public void CreateGUI()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathToWindow);
        var child = tree.Instantiate();
        rootVisualElement.Add(child);
        
        rootVisualElement.Q<TextField>("Searcher").RegisterCallback<ChangeEvent<string>>(x=>UpdateListScript(x.newValue));
        _scrollScript = rootVisualElement.Q<ScrollView>("Scripts");
        
        UpdateListScript(null);
    }

    private void UpdateListScript([CanBeNull]string search)
    {
        if (search == null) SpawnScriptsList(_datas);
        else SpawnScriptsList(_datas.Where(x => x.RelitivePath.Contains(search)));
    }

    private void SpawnScriptsList(IEnumerable<Data> datas)
    {
        _scrollScript.contentContainer.hierarchy.Clear();
        datas.ForEach(data =>
        {
            var label = new Label($"{data.Asset.name} : {data.RelitivePath}");
            label.style.fontSize = 12;
            var startColorFont = label.style.color;
            label.RegisterCallback<MouseOverEvent>(x=>label.style.color = new StyleColor(Color.green));
            label.RegisterCallback<MouseOutEvent>(x=>label.style.color = startColorFont);
            label.RegisterCallback<MouseDownEvent>(y =>
            {
                if (y.button == 0)
                {
                    _callback?.Invoke(data);
                    Close();
                }
            });
            _scrollScript.contentContainer.hierarchy.Add(label);
        });
    }


    public class Data
    {
        public Data(string relitivepath, TextAsset asset)
        {
            RelitivePath = relitivepath;
            Asset = asset;
        }
        
        public string RelitivePath { get; private set; }
        public TextAsset Asset { get; private set; }
    }
}

public class SearchCsModelScript : EditorWindow
{
    private static Type[] TypesForView;
    public static string Path => "Assets/1  - Core/MVP/Editor/UXML/WindowSearchCs.uxml";
    
    private ScrollView _scroll;
    private Dictionary<string, ViewFolder.Path> _folders = new Dictionary<string, ViewFolder.Path>();
    private static Action<Type> _callbackTypeSelected;

    [InitializeOnLoadMethod]
    public static void ReshreshType()
    {
        TypesForView = typeof(CsEn.BaseScript).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(CsEn.BaseScript)) && !x.IsAbstract).ToArray();
    }
    
    public static void Show(Action<Type> callback)
    {
        _callbackTypeSelected = callback;
        var win = CreateInstance<SearchCsModelScript>();
        win.ShowAuxWindow();
    }

    public void CreateGUI()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path);
        var child = tree.Instantiate();
        _scroll = child.Q<ScrollView>("ViewElelemnts");
        
        TypesForView.ForEach(x =>
        {
            var pathAtr = x.GetCustomAttribute<CustomPath>();
            if (pathAtr == null) return;
            var parts = pathAtr.Path.Split('/');
            ViewFolder.Path lastPath = null;
            parts.ForEach(x =>
            {
                if (lastPath == null) lastPath = ViewFolder.Path.GetOrCreate(x, _folders, _scroll.contentContainer);
                else lastPath = lastPath.GetOrCreate(x);
            });
        });

        TypesForView.ForEach(x =>
        {
            var pathAtr = x.GetCustomAttribute<CustomPath>();
            var viewCs = new ViewCsModel(x);
            viewCs.Clicked += () =>
            {
                _callbackTypeSelected?.Invoke(viewCs.ViewType);
                Close();
            };
            if (pathAtr == null)
            {
                _scroll.contentContainer.Add(viewCs);
            }
            else
            {
                ViewFolder.Path lastPath = null;
                pathAtr.Path.Split('/').ForEach(name =>
                {
                    if (lastPath == null) lastPath = ViewFolder.Path.GetOrCreate(name, _folders, _scroll.contentContainer);
                    else lastPath = lastPath.GetOrCreate(name);
                });
                lastPath.Folder.AddContent(viewCs);
            }
        });

        rootVisualElement.Add(child);
    }
}

public class ViewCsModel : VisualElement
{
    public event Action Clicked;
    
    public string Path = "Assets/1  - Core/MVP/Editor/UXML/ModelViews/Parts/CsModelView.uxml";
    private Label _nameLabel;
    private Label _infoLabel;
    private Type _viewType;
    public Type ViewType => _viewType;

    public ViewCsModel(Type type)
    {
        _viewType = type;
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path).Instantiate();
        _nameLabel = tree.Q<Label>("NameScript");
        _infoLabel = tree.Q<Label>("InfoScript");
        tree.RegisterCallback<MouseOverEvent>(x=>tree.style.backgroundColor = new Color(0.3f, 1f, 0.13f, 0.78f));
        tree.RegisterCallback<MouseOutEvent>(x=>tree.style.backgroundColor = new Color(0,0, 0, 0));
        tree.RegisterCallback<MouseDownEvent>(x=>{
            if (x.button == 0)
            {
                Clicked?.Invoke();
            }});
        if (!SetNewView(type))
        {
            Debug.LogWarning("Не могу отобразить - "+type.Name);
        }
        Add(tree);
    }

    public bool SetNewView(Type type)
    {
        if (type.IsAbstract || !type.IsSubclassOf(typeof(CsEn.BaseScript))) return false;
        _nameLabel.text = type.GetCustomAttribute<CustomId>() != null ? $"{type.GetCustomAttribute<CustomId>().Id} ({type.Name})" : type.Name;
        var infoAtr = type.GetCustomAttribute<Info>();
        if (infoAtr == null) RootEditor.SetShow(_infoLabel, false);
        else _infoLabel.text = infoAtr.Value;
        return true;
    }
}


public class ViewFolder : VisualElement
{
    private VisualElement _content;
    private Label _nameLabel;
    private static string PathToBiew => "Assets/1  - Core/MVP/Editor/UXML/ModelViews/Parts/FolderView.uxml";
    private static string OpenIcon => "Assets/1  - Core/MVP/Editor/Images/OpenFolder.png";
    private static string CloseIcon => "Assets/1  - Core/MVP/Editor/Images/CloseFolder.png";

    public string Name
    {
        get => _nameLabel.text;
        set => _nameLabel.text = value;
    }

    public ViewFolder(string name)
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PathToBiew).Instantiate();
        _content = tree.Q("Content");
        tree.Q("InfoPanel").RegisterCallback<MouseDownEvent>(x=>  ChangeViewContent(tree) );
        ChangeViewContent(tree);
        tree.Q<Label>("NameLabel").text = name;
        _nameLabel = tree.Q<Label>("NameLabel"); 
        Add(tree);
    }

    private static void ChangeViewContent(TemplateContainer tree)
    {
        RootEditor.HideShow(tree.Q("Content"));
        tree.Q("Icon").style.backgroundImage = new StyleBackground(
            tree.Q("Content").style.display == DisplayStyle.Flex
                ? AssetDatabase.LoadAssetAtPath<Texture2D>(OpenIcon)
                : AssetDatabase.LoadAssetAtPath<Texture2D>(CloseIcon));
    }

    public void AddContent(VisualElement eleemnt) => _content.Add(eleemnt);

    public void ClearContent() => _content.hierarchy.Clear();

    public class Path
    {
        public string Value => Folder.Name;
        public ViewFolder Folder { get; private set; }

        private Dictionary<string, Path> _otherFolder = new Dictionary<string, Path>();
        
        public Path(ViewFolder folder) => Folder = folder;

        public Path GetOrCreate(string name) => GetOrCreate(name, _otherFolder, Folder._content);

        public static Path GetOrCreate(string name, Dictionary<string, Path> otherDicts, VisualElement parentForAdd)
        {
            if (otherDicts.ContainsKey(name)) return otherDicts[name];
            var newFolder = new ViewFolder(name);
            otherDicts.Add(name, new Path(newFolder));
            parentForAdd.Add(newFolder);
            return otherDicts[name];
        }
    }
}