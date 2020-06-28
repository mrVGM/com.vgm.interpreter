using System;
using ScriptingLaunguage.Interpreter;
using ScriptingLaunguage.Parser;
using ScriptingLaunguage.Tokenizer;
using UnityEditor;
using UnityEngine;

namespace ScriptingLaunguage
{
    public class InterpreterWindow : EditorWindow
    {
        private const string EditorPrefsKey = "c_sharp_interpreter_scripts_location";
        
        [HideInInspector]
        [SerializeField]
        private TextAsset _parserTable;
        private string _scriptsFolder = "";

        private ParserTable __table;

        ParserTable _table
        {
            get
            {
                if (__table == null)
                {
                    __table = ParserTable.Deserialize(_parserTable.bytes);
                }
                return __table;
            }
        }


        private Session _session;
        private Interpreter.Interpreter _interpreter;
        
        private void CreateSession()
        {
            var parser = new Parser.Parser { ParserTable = _table };
            _session = new Session(_scriptsLocation, parser);
            _interpreter = new Interpreter.Interpreter(_session);
        }
        
        private string _buffer = "";
        private string _command = "";
        
        private string _scriptsLocation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_scriptsFolder))
                {
                    if (EditorPrefs.HasKey(EditorPrefsKey))
                    {
                        _scriptsFolder = EditorPrefs.GetString(EditorPrefsKey);
                    }
                }

                return _scriptsFolder;
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrWhiteSpace(_scriptsFolder) && EditorPrefs.HasKey(EditorPrefsKey))
            {
                _scriptsFolder = EditorPrefs.GetString(EditorPrefsKey);
            }

            GUILayout.Label("Scripts Location");
            _scriptsFolder = GUILayout.TextField(_scriptsFolder);
            
            GUILayout.Label("Command");
            _command = GUILayout.TextField(_command);

            if (Event.current.type == EventType.KeyDown &&
                Event.current.character == '\n')
            {
                if (string.IsNullOrWhiteSpace(_command))
                {
                    _command = "";
                    _buffer = "";
                    return;
                }

                Debug.Log(_command);
                _buffer += $"{Environment.NewLine}{_command}";
                _command = "";

                if (_interpreter == null)
                {
                    CreateSession();
                }

                var scriptId = new ScriptId {Script = _buffer, Filename = ""};
                try
                {
                    var res = _interpreter.RunScript(_buffer, _session.GetWorkingScope(), scriptId);
                    Debug.Log($"{(res == null ? "null" : res)}");
                    _buffer = "";
                }
                catch (Utils.LanguageException e)
                {
                    var parseException = e as Parser.Parser.IParseException;
                    if (parseException != null && e.CodeIndex == _buffer.Length)
                    {
                        Debug.Log("...");
                    }
                    else
                    {
                        _buffer = "";
                        throw e;
                    }
                }
            }

            if (GUILayout.Button("Reset Session"))
            {
                CreateSession();
            }
            
            if (GUILayout.Button("Save Scripts Location"))
            {
                EditorPrefs.SetString(EditorPrefsKey, _scriptsFolder);
            }

            Repaint();
        }
        
        [MenuItem("Window/Scripts Interpreter")]
        public static void ShowWindow()
        {
            GetWindow<InterpreterWindow>();
        }
    }
}
