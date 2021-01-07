using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptingLanguage.VisualScripting
{
    public class PropertyExplorer : MonoBehaviour
    {
        public Button InfoPrefab;

        public GameObject Property;
        public RectTransform PropertiesRoot;

        private static Type[] _types;
        private static string[] _namespaces;

        private static Dictionary<string, List<string>> _optionsByProperties = new Dictionary<string, List<string>>();

        private bool justCreated = true;

        public void OnEnable()
        {
            if (justCreated) {
                return;
            }
            UpdateButtons();
        }

        private static IEnumerable<string> GetOptionsStatic(IEnumerable<string> query)
        {
            var queryString = "";
            if (query.Any())
            {
                queryString = query.Aggregate((x, y) => x + "." + y);
            }
            List<string> res;
            if (_optionsByProperties.TryGetValue(queryString, out res)) {
                return res;
            }

            while (_optionsByProperties.Count() > 100) {
                var key = _optionsByProperties.Keys.FirstOrDefault();
                _optionsByProperties.Remove(key);
            }

            var availablePaths = Types.Select(x => x.FullName.Split('.')).Where(x => OptionCompatible(x, query));
            var staticPaths = GenerateStaticPropertiesPaths(query).Distinct();
            var options = availablePaths.Select(x => x.Skip(query.Count()).FirstOrDefault()).Distinct().ToList();
            options.AddRange(staticPaths);
            _optionsByProperties[queryString] = options;
            return options;
        }

        public static IEnumerable<Type> Types
        {
            get
            {
                if (_types == null)
                {
                    IEnumerable<Type> getTypes() {
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies) 
                        {
                            foreach (var type in assembly.GetTypes()) {
                                yield return type;
                            }
                        }
                    }

                    _types = getTypes().ToArray();
                }

                return _types;
            }
        }

        static IEnumerable<string> GenerateStaticPropertiesPaths(IEnumerable<string> propertiesPath)
        {
            IEnumerable<string> findOptions(Type t, IEnumerable<string> reminder)
            {
                IEnumerable<MemberInfo> members = t.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (!reminder.Any())
                {
                    foreach (var member in members)
                    {
                        yield return member.Name;
                    }
                    yield break;
                }

                var name = reminder.FirstOrDefault();
                reminder = reminder.Skip(1);

                members = members.Where(x => x.Name == name);

                foreach (var member in members)
                {
                    if (member is FieldInfo)
                    {
                        var fieldInfo = member as FieldInfo;
                        var results = findOptions(fieldInfo.FieldType, reminder);
                        foreach (var res in results)
                        {
                            yield return res;
                        }
                        continue;
                    }
                    if (member is PropertyInfo)
                    {
                        var propertyInfo = member as PropertyInfo;
                        var results = findOptions(propertyInfo.PropertyType, reminder);
                        foreach (var res in results)
                        {
                            yield return res;
                        }
                        continue;
                    }
                }
            }

            var propertyPathStr = "";
            if (propertiesPath.Any()) {
                propertyPathStr = propertiesPath.Aggregate((x, y) => x + "." + y);
            }

            var types = Types.Where(x => propertyPathStr.StartsWith(x.FullName));
            foreach (var type in types)
            {
                var typePath = type.FullName.Split('.');
                var propertyPathReminder = propertiesPath.Skip(typePath.Count());

                var options = findOptions(type, propertyPathReminder);
                foreach (var option in options) {
                    yield return option;
                }
            }
        }

        public IEnumerable<string> Namespaces
        {
            get
            {
                if (_namespaces == null) {
                    _namespaces = Types.Select(x => x.Namespace).Distinct().ToArray();
                }
                return _namespaces;
            }
        }

        public PropertiesNodeComponent PropertiesNode => GetComponentInParent<PropertiesNodeComponent>();
        public IEnumerable<string> CurrentProperties 
        {
            get
            {
                int childCount = PropertiesNode.PropertiesRoot.childCount;
                for (int i = 0; i < childCount; ++i) {
                    var cur = PropertiesNode.PropertiesRoot.GetChild(i);
                    if (cur.gameObject == Property) {
                        break;
                    }
                    var text = cur.GetComponentInChildren<Text>();
                    yield return text.text;
                }
            }
        }

        private void UpdateButtons(IEnumerable<string> names)
        {
            int neededButtons = names.Count();
            IEnumerable<Button> getButtons() 
            {
                int childCount = PropertiesRoot.childCount;
                for (int i = 0; i < childCount; ++i) {
                    var cur = PropertiesRoot.GetChild(i);
                    cur.gameObject.SetActive(false);
                }

                for (int i = 0; i < neededButtons; ++i) {
                    if (i < childCount) {
                        var curChild = PropertiesRoot.GetChild(i);
                        curChild.gameObject.SetActive(true);
                        yield return curChild.GetComponent<Button>();
                        continue;
                    }

                    var button = Instantiate(InfoPrefab, PropertiesRoot);
                    yield return button;
                }
            }

            var inputField = Property.GetComponentInChildren<InputField>();
            Enumerable.Zip(getButtons(), names, (button, name) => {
                var text = button.GetComponentInChildren<Text>();
                text.text = name;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    inputField.text = name;
                });
                return button;
            }).ToList();
        }

        private IEnumerable<string> GetOptions(object obj, IEnumerable<string> query)
        {
            var genericObject = obj as Interpreter.GenericObject;
            if (genericObject != null) {
                return genericObject.GetProperties();
            }
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            IEnumerable<string> getOptions(Type type, IEnumerable<string> path) {
                IEnumerable<MemberInfo> members = type.GetMembers(flags);
                if (!path.Any()) {
                    foreach (var member in members) {
                        yield return member.Name;
                    }
                    yield break;
                }

                var nextMemberName = path.First();
                path = path.Skip(1);
                members = members.Where(x => x.Name == nextMemberName);

                var fields = members.OfType<FieldInfo>();
                var properties = members.OfType<PropertyInfo>();

                foreach (var field in fields) {
                    var options = getOptions(field.FieldType, path);
                    foreach (var opt in options) {
                        yield return opt;
                    }
                }
                foreach (var property in properties) {
                    var options = getOptions(property.PropertyType, path);
                    foreach (var opt in options) {
                        yield return opt;
                    }
                }
            }

            if (obj == null) {
                return Enumerable.Empty<string>();
            }

            var objType = obj.GetType();
            var res = getOptions(objType, query);
            return res.Distinct();
        }

        private static bool OptionCompatible(IEnumerable<string> optionPath, IEnumerable<string> propertiesPath)
        {
            if (optionPath.Count() <= propertiesPath.Count()) {
                return false;
            }

            var zip = Enumerable.Zip(optionPath, propertiesPath, (pathStr, propertyStr) => pathStr == propertyStr);
            return zip.All(x => x);
        }

        private object GetLinkedObject() 
        {
            var endpoint = PropertiesNode.ObjectEndpoint.Endpoint.LinkedEndpoints.FirstOrDefault();
            if (endpoint == null) {
                return null;
            }

            var frame = GetComponentInParent<Frame>();
            var node = frame.NodesDB.GetNodeByEndpoint(endpoint) as VariableNodeComponent.VariableNode;
            if (node == null) {
                return null;
            }

            var scope = frame.VisualScriptingSession.GetRootScope();
            var obj = scope.GetVariable(node.VariableName);
            return obj;
        }

        public void UpdateButtons()
        {
            var options = Enumerable.Empty<string>();

            if (PropertiesNode.ObjectEndpoint.Endpoint.LinkedEndpoints.Any()) {
                var obj = GetLinkedObject();
                options = GetOptions(obj, CurrentProperties);
            } else {
                options = GetOptionsStatic(CurrentProperties);
            }

            var inputField = Property.GetComponentInChildren<InputField>();
            var str = inputField.text;
            if (!string.IsNullOrWhiteSpace(str)) {
                options = options.Where(x => x.StartsWith(str, StringComparison.OrdinalIgnoreCase));
            }

            UpdateButtons(options);
        }
    }
}