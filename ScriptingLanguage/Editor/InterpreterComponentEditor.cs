using System;
using ScriptingLaunguage.Interpreter;
using ScriptingLaunguage.Tokenizer;
using UnityEditor;
using UnityEngine;

namespace ScriptingLaunguage
{
    [CustomEditor(typeof(InterpreterComponent))]
    public class InterpreterComponentEditor : Editor
    {
        private const string EditorPrefsKey = "c_sharp_interpreter_scripts_location";
        private InterpreterComponent _interpreterComponent => target as InterpreterComponent;
        private void CreateSession()
        {
            _interpreterComponent.Session = new Session(_interpreterComponent.ScriptsFolder);
            _interpreterComponent.Interpreter = new Interpreter.Interpreter(_interpreterComponent.Session);
        }

        
        private Interpreter.Interpreter _interpreter 
        {
            get
            {
                if (_interpreterComponent.Session == null) {
                    CreateSession();
                }

                return _interpreterComponent.Interpreter;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (string.IsNullOrWhiteSpace(_interpreterComponent.ScriptsFolder) && EditorPrefs.HasKey(EditorPrefsKey)) {
                _interpreterComponent.ScriptsFolder = EditorPrefs.GetString(EditorPrefsKey);
            }

            if (GUILayout.Button("Evaluate")) {
                if (string.IsNullOrWhiteSpace(_interpreterComponent.Command)) {
                    return;
                }

                _interpreterComponent.Output += $"{Environment.NewLine}{_interpreterComponent.Command}";

                var scriptId = new ScriptId { Script = _interpreterComponent.Command, Filename = "" };
                try {
                    var res = _interpreter.RunScript(_interpreterComponent.Command, _interpreterComponent.Session.GetWorkingScope(), scriptId);
                    _interpreterComponent.Output += $"{Environment.NewLine}{(res != null ? res : "null")}";
                }
                catch (Utils.LanguageException e) {
                    var parseException = e as Parser.Parser.IParseException;
                    if (parseException != null) {
                        _interpreterComponent.Output += $"{Environment.NewLine}{e.Message}";
                    }

                    _interpreterComponent.Command = "";
                    throw e;
                }
                _interpreterComponent.Command = "";
                return;
            }

            if (GUILayout.Button("Reset Session")) {
                CreateSession();
                _interpreterComponent.Command = "";
                _interpreterComponent.Output = "";
                return;
            }

            if (GUILayout.Button("Save Scripts In Preferences")) {
                EditorPrefs.SetString(EditorPrefsKey, _interpreterComponent.ScriptsFolder);
            }
        }
    }
}
