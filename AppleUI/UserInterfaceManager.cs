using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AppleSerialization;
using AppleUI.Interfaces.Behavior;
using FastDeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AppleUI
{
    /// <summary>
    /// Provides an easy-to-use interface to manage and interact with user interface objects.
    /// </summary>
    public sealed class UserInterfaceManager : IDisposable
    {
        /// <summary>
        /// <see cref="Panel"/> objects that are to be displayed.
        /// </summary>
        public List<(string Name, Panel Panel)> PanelsCurrentlyDisplayed { get; private set; }
        
        /// <summary>
        /// <see cref="Panel"/> objects loaded through the
        /// <see cref="UserInterfaceManager(GraphicsDevice, SerializationSettings, Assembly, string[])"/> constructor,
        /// with their names being the key to this dictionary.
        /// </summary>
        public Dictionary<string, Panel> Panels { get; private set; }
        
        /// <summary>
        /// <see cref="Assembly"/> containing classes of user-defined behavior scripts detailing the behavior of
        /// specific UI elements.
        /// </summary>
        public Assembly ScriptAssembly { get; private set; }

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Constructs a <see cref="UserInterfaceManager"/> object.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance that will draw the user interface and
        /// create graphical resources.</param>
        /// <param name="serializationSettings">Provides additional information/data necessary to deserialize
        /// the UI panel files.</param>
        /// <param name="scriptAssembly"><see cref="Assembly"/> containg classes that represent scripts defining the
        /// behavior of UI elements. </param>
        /// <param name="absolutePathsToPanelFiles">Absolute paths to json files describing UI panels
        /// (extension does not have to be .json, but must be json files). If the file does not exist, then it will
        /// be ignored and not loaded. </param>
        public UserInterfaceManager(GraphicsDevice graphicsDevice, SerializationSettings serializationSettings,
            Assembly scriptAssembly, params string[] absolutePathsToPanelFiles)
        {
#if DEBUG
            const string constructorName = $"{nameof(UserInterfaceManager)} constructor (params string[])";
#endif
            PanelsCurrentlyDisplayed = new List<(string Name, Panel Panel)>();
            Panels = new Dictionary<string, Panel>();
            ScriptAssembly = scriptAssembly;

            foreach (string absolutePath in absolutePathsToPanelFiles)
            {
                if (!File.Exists(absolutePath))
                {
#if DEBUG
                    Debug.WriteLine($"{constructorName}: cannot find panel of path {absolutePath}. Skipping.");
#endif
                    continue;
                }

                string panelName = Path.GetFileNameWithoutExtension(absolutePath);
                string panelFileContents = File.ReadAllText(absolutePath);

                Utf8JsonReader jsonReader = new(Encoding.UTF8.GetBytes(panelFileContents),
                    new JsonReaderOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                Panel? panel =
                    Serializer.Deserialize<Panel>(ref jsonReader, serializationSettings, JsonSerializerOptions);

                if (panel is null)
                {
#if DEBUG
                    Debug.WriteLine($"{constructorName}: unable to create panel of path {absolutePath}. Skipping.");
#endif
                    continue;
                }

                panel.GraphicsDevice = graphicsDevice;
                panel.Manager = this;
                panel.LoadAllScripts();

                Panels.Add(panelName, panel);
            }
        }

        /// <summary>
        /// Updates all <see cref="Panel"/>s in <see cref="PanelsCurrentlyDisplayed"/>
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> object to pass to the <see cref="Panel.Update(GameTime)"/>
        /// methods of each panel in <see cref="PanelsCurrentlyDisplayed"/>.</param>
        public void UpdateDisplayedPanels(GameTime gameTime)
        {
            foreach (var (_, panel) in PanelsCurrentlyDisplayed)
            {
                panel.Update(gameTime);
            }
        }

        /// <summary>
        /// Draws all <see cref="Panel"/>s in <see cref="PanelsCurrentlyDisplayed"/>
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> object to pass to the
        /// <see cref="Panel.Draw(GameTime, SpriteBatch)"/> methods of each panel in <see cref="PanelsCurrentlyDisplayed"/>.
        /// </param>
        /// <param name="spriteBatch"><see cref="SpriteBatch"/> object used to draw the <see cref="Panel"/>s in
        /// <see cref="PanelsCurrentlyDisplayed"/>.</param>
        public void DrawDisplayedPanels(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var (_, panel) in PanelsCurrentlyDisplayed)
            {
                panel.Draw(gameTime, spriteBatch);
            }
        }

        private static readonly FastDeepClonerSettings DeepClonerSettings = new();

        /// <summary>
        /// Clones a <see cref="Panel"/> from <see cref="Panels"/> and inserts into the
        /// <see cref="PanelsCurrentlyDisplayed"/>, thus displaying it.
        /// </summary>
        /// <param name="panelName">Name of the panel in <see cref="Panels"/> to display.</param>
        /// <returns>If the panel of name <see cref="panelName"/> cannot be found, then null is returned. Otherwise,
        /// a clone of the panel with name <see cref="panelName"/> in <see cref="Panels"/> is returned.</returns>
        public Panel? TryDisplayPanel(string panelName)
        {
            if (!Panels.TryGetValue(panelName, out Panel? panel))
            {
                Debug.WriteLine($"{nameof(UserInterfaceManager)}.{nameof(TryDisplayPanel)}: cannot find panel " +
                                $"of name {panelName}.");
                return null;
            }

            Panel panelClone = (Panel) panel.Clone();
            PanelsCurrentlyDisplayed.Add((panelName, panelClone));

            return panel;
        }

        /// <summary>
        /// Clones a <see cref="Panel"/> from <see cref="Panels"/> and inserts into the
        /// <see cref="PanelsCurrentlyDisplayed"/>, thus displaying it.
        /// </summary>
        /// <param name="panelName">Name of the panel in <see cref="Panels"/> to display.</param>
        /// <returns>If the panel of name <see cref="panelName"/> cannot be found, then a
        /// <see cref="KeyNotFoundException"/> is thrown. Otherwise, a clone of the panel with name
        /// <see cref="panelName"/> in <see cref="Panels"/> is returned.</returns>
        public Panel DisplayPanel(string panelName)
        {
            Panel panelClone = (Panel) Panels[panelName].Clone();
            PanelsCurrentlyDisplayed.Add((panelName, panelClone));

            return panelClone;
        }

        /// <summary>
        /// Removes a <see cref="Panel"/> from <see cref="PanelsCurrentlyDisplayed"/>, thus removing it from view and
        /// "closing" it. Additionally, the panel is also disposed. Multiple panels can be close since multiple panels
        /// can share the same name in <see cref="PanelsCurrentlyDisplayed"/>.
        /// </summary>
        /// <param name="panelName">The name of the panel to close.</param>
        /// <returns>The number of panels that were closed.</returns>
        public int CloseDisplayedPanel(string panelName) => CloseDisplayedPanel((panelName, null), true);
        
        /// <summary>
        /// Removes a <see cref="Panel"/> from <see cref="PanelsCurrentlyDisplayed"/>, thus removing it from view and
        /// "closing" it. Additionally, the panel is also disposed. Multiple panels can be close since multiple panels
        /// can share the same panel object in <see cref="PanelsCurrentlyDisplayed"/> (although ideally this shouldn't
        /// happen as it wastes space and can cause complications).
        /// </summary>
        /// <param name="panel">The panel object to close.</param>
        /// <returns>The number of panels that were closed.</returns>
        public int CloseDisplayedPanel(Panel panel) => CloseDisplayedPanel((null, panel), false);

        private int CloseDisplayedPanel((string? PanelName, Panel? Panel) panel, bool compareNames)
        {
            int panelsClosed = 0;
            
            for (int i = PanelsCurrentlyDisplayed.Count - 1; i > -1; i--)
            {
                var (currentPanelName, currentPanel) = PanelsCurrentlyDisplayed[i];

                if (compareNames ? panel.PanelName == currentPanelName : panel.Panel == currentPanel)
                {
                    currentPanel.Dispose();
                    PanelsCurrentlyDisplayed.RemoveAt(i);
                    
                    panelsClosed++;
                }
            }

            return panelsClosed;
        }

        internal IElementBehaviorScript[] LoadElementBehaviorScripts(ElementScriptInfo[] scriptInfos,
            params Type[] requiredInterfaces)
        {
            List<IElementBehaviorScript> outScripts = new();

            foreach (ElementScriptInfo scriptInfo in scriptInfos)
            {
                IElementBehaviorScript? script = LoadElementBehaviorScript(scriptInfo, requiredInterfaces);
                if (script is not null) outScripts.Add(script);
            }

            return outScripts.ToArray();
        }
        
        internal IElementBehaviorScript? LoadElementBehaviorScript(ElementScriptInfo scriptInfo,
            params Type[] requiredInterfaces)
        {
#if DEBUG
            const string methodName = nameof(UserInterfaceManager) + "." + nameof(LoadElementBehaviorScript);
#endif
            string typeName = $"UserInterfaceScripts._{scriptInfo.Name}";
            Type? scriptType = ScriptAssembly.GetType(typeName);

            if (scriptType is null)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: cannot find script of name _{scriptInfo.Name}.");
#endif
                return null;
            }

            StringBuilder missingInterfaceNames = new();
            Type[] scriptInterfaces = scriptType.GetInterfaces();

            foreach (Type interfaceType in requiredInterfaces)
            {
                if (!scriptInterfaces.Contains(interfaceType))
                {
                    missingInterfaceNames.Append($"{interfaceType}, ");
                }
            }

            if (!scriptInterfaces.Contains(typeof(IElementBehaviorScript)))
            {
                missingInterfaceNames.Append($"{nameof(IElementBehaviorScript)}, ");
            }

            if (missingInterfaceNames.Length > 0)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: script of name {scriptInfo.Name} does not implement the required " +
                                $"interfaces: {missingInterfaceNames.ToString()[..^2]}");
#endif
                return null;
            }
            
            ConstructorInfo? scriptArgsConstructor = scriptType.GetConstructors().Where(c =>
            {
                ParameterInfo[] parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(Dictionary<string, object>);
            }).FirstOrDefault();

            if (scriptArgsConstructor is null)
            {
#if DEBUG
                Debug.WriteLine($"{methodName}: script of name {scriptInfo} does not have a constructor with one" +
                                $" parameter accepting a Dictionary<string, object> object.");
#endif
                return null;
            }

            IElementBehaviorScript script = (IElementBehaviorScript) scriptType.CreateInstance(scriptInfo.Arguments);

            script.Arguments = scriptInfo.Arguments;
            script.Enabled = scriptInfo.Enabled;

            if (!script.AreArgumentsValid())
            {
                Debug.WriteLine($"{methodName}: script of name {scriptInfo.Name} has invalid arguments.");
                return null;
            }

            return script;
        }

        /// <summary>
        /// Disposes this object. All panels in <see cref="PanelsCurrentlyDisplayed"/> and <see cref="Panels"/> are
        /// disposed as well.
        /// </summary>
        public void Dispose()
        {
            foreach (var panel in PanelsCurrentlyDisplayed)
            {
                panel.Panel.Dispose();
            }

            PanelsCurrentlyDisplayed.Clear();

            foreach (Panel panel in Panels.Values)
            {
                panel.Dispose();
            }
            
            Panels.Clear();
        }
    }
}