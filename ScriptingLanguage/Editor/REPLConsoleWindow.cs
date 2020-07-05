using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScriptingLaunguage.Interpreter;
using ScriptingLaunguage.Parser;
using ScriptingLaunguage.Tokenizer;
using UnityEditor;
using UnityEngine;

namespace ScriptingLaunguage
{
    public class REPLConsoleWindow : EditorWindow
    {
        internal class CommandsBuffer
        {
            private const int size = 100;
            List<string> commands = new List<string>(100);

            public void Add(string command)
            {
                while (commands.Count >= size)
                {
                    commands.RemoveAt(0);
                }

                commands.Add(command);
            }

            public string this[int index]
            {
                get => commands[index];
            }

            public int Count => commands.Count;
        }

        private const string EditorPrefsKey = "c_sharp_interpreter_scripts_location";

        private ParserTableData _parserTableData;
        private ParserTableData parserTableData 
        {
            get 
            {
                if (_parserTableData == null) {
                    string guid = AssetDatabase.FindAssets($"t:{typeof(ParserTableData).Name}").FirstOrDefault();
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    _parserTableData = AssetDatabase.LoadAssetAtPath<ParserTableData>(path);
                }

                return _parserTableData;
            }
        }

        private Session session = null;
        private Interpreter.Interpreter interpreter = null;


        private Vector2 scrollPos;
        private string consoleOutput = "";
        private string command;

        private CommandsBuffer commandsBuffer = new CommandsBuffer();
        private int index = -1;

        private string buffer = "";

        private GUIStyle labelStyle = new GUIStyle();

        private string scriptsLocation = null;

        private ParserTable _parserTable;

        private ParserTable parserTable
        {
            get
            {
                if (_parserTable == null)
                {
                    _parserTable = Parser.ParserTable.Deserialize(parserTableData.ParserTable.bytes);
                }

                return _parserTable;
            }
        }

        private void CreateSession()
        {
            var parser = new Parser.Parser {ParserTable = parserTable};
            session = new Session(scriptsLocation, parser);
            interpreter = new Interpreter.Interpreter(session);
        }

        void ScrollToTheEnd()
        {
            float height = labelStyle.CalcHeight(new GUIContent(consoleOutput), position.width);
            scrollPos = height * Vector2.up;
        }

        private MethodInfo[] _metaCommands;

        private IEnumerable<MethodInfo> metaCommands
        {
            get
            {
                if (_metaCommands == null)
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var types = assemblies.SelectMany(x => x.GetTypes());
                    types = new[] {typeof(REPLConsoleWindow)};
                    var methods = types.SelectMany(x =>
                        x.GetMethods());
                    methods = methods.Where(x => x.GetCustomAttribute<MetaCommandAttribute>() != null);
                    _metaCommands = methods.ToArray();
                }

                return _metaCommands;
            }
        }

        private void OnGUI()
        {
            var e = Event.current;
            scrollPos =
                EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width),
                    GUILayout.Height(position.height - 20));
            GUILayout.Label(consoleOutput, labelStyle);
            EditorGUILayout.EndScrollView();
            command = EditorGUILayout.TextField(command);

            if (e.type == EventType.KeyDown)
            {
                int commandsInBuffer = commandsBuffer.Count;
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        string cmd = "";
                        if (!string.IsNullOrWhiteSpace(command)) 
                        {
                            cmd = command;
                        }
                        cmd = cmd.Trim();
                        command = "";
                        if (string.IsNullOrWhiteSpace(cmd))
                        {
                            buffer = "";
                            break;
                        }

                        commandsBuffer.Add(cmd);
                        index = commandsBuffer.Count;
                        if (cmd.StartsWith(":"))
                        {
                            var args = cmd.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));
                            string cmdName = args.First().Substring(1);

                            var commandMethod = metaCommands.FirstOrDefault(x =>
                                x.GetCustomAttribute<MetaCommandAttribute>().Name == cmdName);

                            if (commandMethod != null)
                            {
                                var stringArgs = args.Skip(1).ToArray();
                                commandMethod.Invoke(null, new object[] { this, stringArgs });
                            }

                            ScrollToTheEnd();
                            break;
                        }
                        
                        consoleOutput += $"{(string.IsNullOrEmpty(consoleOutput) ? "" : "\n")}> {cmd}";
                        buffer += $"{(string.IsNullOrEmpty(buffer) ? "" : Environment.NewLine)}{cmd}";
                        ScrollToTheEnd();

                        var scriptId = new ScriptId {Filename = "", Script = buffer};
                        try
                        {
                            var res = interpreter.RunScript(buffer, session.GetWorkingScope(), scriptId);
                            consoleOutput += $"\n> {(res == null ? "null" : res)}";
                            buffer = "";
                        }
                        catch (Utils.LanguageException exception)
                        {
                            var parseException = exception as Parser.Parser.IParseException;
                            if (parseException != null
                                && exception.ScriptId == scriptId
                                && exception.CodeIndex == buffer.Length)
                            {
                                consoleOutput += $"\n...";
                            }
                            else
                            {
                                consoleOutput += $"\n{exception.Message}";
                                ScrollToTheEnd();
                                buffer = "";
                                throw exception;
                            }
                        }
                        ScrollToTheEnd();
                        break;
                    case KeyCode.UpArrow:
                        if (commandsInBuffer == 0)
                            break;

                        index = Mathf.Clamp(--index, 0, commandsInBuffer - 1);
                        command = commandsBuffer[index];
                        break;

                    case KeyCode.DownArrow:
                        if (commandsInBuffer == 0)
                            break;

                        index = Mathf.Clamp(++index, 0, commandsInBuffer);
                        if (index < commandsInBuffer)
                            command = commandsBuffer[index];
                        else
                            command = "";

                        break;
                }
            }

            Repaint();
        }

        [MenuItem("Window/REPL Console")]
        public static void ShowWindow()
        {
            var consoleWindow = GetWindow<REPLConsoleWindow>("REPL Console");
            if (EditorPrefs.HasKey(EditorPrefsKey))
            {
                consoleWindow.scriptsLocation = EditorPrefs.GetString(EditorPrefsKey);
            }

            if (consoleWindow.scriptsLocation == null) 
            {
                consoleWindow.scriptsLocation = "";
                if (EditorPrefs.HasKey(EditorPrefsKey)) 
                {
                    consoleWindow.scriptsLocation = EditorPrefs.GetString(EditorPrefsKey);
                }
            }
            consoleWindow.CreateSession();
        }

        public void Print(string message)
        {
            consoleOutput += $"{(string.IsNullOrEmpty(consoleOutput) ? "" : "\n")}> {message}";
            ScrollToTheEnd();
        }

        #region Meta Commands

        [MetaCommand(Name = "reset_session")]
        public static void ResetSession(REPLConsoleWindow console, params string[] args)
        {
            if (args.Length == 1)
            {
                console.scriptsLocation = args[0];
            }

            console.CreateSession();
            console.Print($"Scripts Location: {(string.IsNullOrWhiteSpace(console.scriptsLocation) ? "N/A" : console.scriptsLocation)}");
        }

        [MetaCommand(Name = "cls")]
        public static void ClearConsole(REPLConsoleWindow console, params string[] args) 
        {
            console.consoleOutput = "";
        }

        [MetaCommand(Name = "scripts_dir")]
        public static void PrintScriptsDirectory(REPLConsoleWindow console, params string[] args)
        {
            console.Print($"Scripts Location: {(string.IsNullOrWhiteSpace(console.scriptsLocation) ? "N/A" : console.scriptsLocation)}");
        }

        [MetaCommand(Name = "save_scripts_dir")]
        public static void SaveScriptsDirectory(REPLConsoleWindow console, params string[] args)
        {
            EditorPrefs.SetString(EditorPrefsKey, console.scriptsLocation);
            console.Print($"Scripts Location: {(string.IsNullOrWhiteSpace(console.scriptsLocation) ? "N/A" : console.scriptsLocation)}");
        }

        #endregion
    }
}