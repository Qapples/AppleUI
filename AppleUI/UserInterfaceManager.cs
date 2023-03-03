using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
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
        /// <see cref="Panel"/> objects loaded through the <see cref="UserInterfaceManager(string[])"/> constructor,
        /// with their names being the key to this dictionary.
        /// </summary>
        public Dictionary<string, Panel> Panels { get; private set; }

        /// <summary>
        /// Constructs a <see cref="UserInterfaceManager"/> object.
        /// </summary>
        /// <param name="absolutePathsToPanelFiles">Absolute paths to json files describing UI panels
        /// (extension does not have to be .json, but must be json files). If the file does not exist, then it will
        /// be ignored and not loaded. </param>
        public UserInterfaceManager(params string[] absolutePathsToPanelFiles)
        {
#if DEBUG
            const string constructorName = $"{nameof(UserInterfaceManager)} constructor (params string[])";
#endif
            
            PanelsCurrentlyDisplayed = new List<(string Name, Panel Panel)>();
            Panels = new Dictionary<string, Panel>();

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

                Utf8JsonReader jsonReader = new(Encoding.UTF8.GetBytes(panelFileContents), new JsonReaderOptions
                    { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                Panel? panel = AppleSerialization.Serializer.Deserialize<Panel>(ref jsonReader);

                if (panel is null)
                {
#if DEBUG
                    Debug.WriteLine($"{constructorName}: unable to create panel of path {absolutePath}. Skipping.");
#endif
                    continue;
                }
                
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

            Panel panelClone = panel.Clone(DeepClonerSettings);
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
            Panel panelClone = Panels[panelName].Clone(DeepClonerSettings);
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