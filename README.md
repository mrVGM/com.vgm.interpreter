# How to integrate the interpreter with Unity

## Importing the package to Unity
1. Checkout the repository and then switch to the `unity_package` branch
2. Open Unity and go to `Window -> Package Manager`
3. Hit the _Plus Sign_ and choose _Add package from disk..._

![Alt text](Images/package_manager.png?raw=true "Window -> Package Manager")

4. Navigate to `<root_of_the_repository>/Scripting` and open the `package.json` file

This will import the package to your Unity project.

## Running scripts
To actually run the scripts from the interpreter you need to:

1. Create a directory somewhere in the file system
2. Copy the `<root_of_the_repository>/ScriptingLanguage/Runtime/GrammarTests/grammar.txt` file into it
3. Create a GameObject and attach the `InterpreterComponent` to it
4. Put the path to the directory, where copied the `grammar.txt` file, into the `ScriptsFolder` field

![Alt text](Images/setting_up_scripts_directory.png?raw=true "InterpreterComponent")

Then all you need to do is type a command and hit the `Evaluate` button :sunglasses:

![Alt text](Images/evaluating_a_command.png?raw=true "InterpreterComponent")

(To store the path to your default scripts directory hit the _Save Scripts In Preferences_ button. This way you won't need to type the path into the _ScriptsFolder_ field again, when you attach a new `InterpreterComponent`)
