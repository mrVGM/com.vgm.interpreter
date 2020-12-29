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

        public void OnEnable()
        {
            UpdateButtons();
        }

        private static IEnumerable<string> GetOptions(IEnumerable<string> query)
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

        private static bool OptionCompatible(IEnumerable<string> optionPath, IEnumerable<string> propertiesPath)
        {
            if (optionPath.Count() <= propertiesPath.Count()) {
                return false;
            }

            var zip = Enumerable.Zip(optionPath, propertiesPath, (pathStr, propertyStr) => pathStr == propertyStr);
            return zip.All(x => x);
        }

        public void UpdateButtons()
        {
            var propertiesNode = GetComponentInParent<PropertiesNodeComponent>();

            if (propertiesNode.ObjectEndpoint.Endpoint.LinkedEndpoints.Any()) {
                UpdateButtons(Enumerable.Empty<string>());
                return;
            }

            var options = GetOptions(CurrentProperties);
            var inputField = Property.GetComponentInChildren<InputField>();
            var str = inputField.text;
            if (!string.IsNullOrWhiteSpace(str)) {
                options = options.Where(x => x.StartsWith(str, StringComparison.OrdinalIgnoreCase));
            }

            UpdateButtons(options);
        }
    }
}