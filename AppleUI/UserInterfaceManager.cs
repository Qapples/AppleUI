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
    public sealed class UserInterfaceManager : IDisposable
    {
        public List<(string Name, Panel Panel)> PanelsCurrentlyDisplayed { get; private set; }
        public Dictionary<string, Panel> Panels { get; private set; }

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

        public void UpdateDisplayedPanels(GameTime gameTime)
        {
            foreach (var (_, panel) in PanelsCurrentlyDisplayed)
            {
                panel.Update(gameTime);
            }
        }

        public void DrawDisplayedPanels(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var (_, panel) in PanelsCurrentlyDisplayed)
            {
                panel.Draw(gameTime, spriteBatch);
            }
        }

        private static readonly FastDeepClonerSettings DeepClonerSettings = new();

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

        public Panel DisplayPanel(string panelName)
        {
            Panel panelClone = Panels[panelName].Clone(DeepClonerSettings);
            PanelsCurrentlyDisplayed.Add((panelName, panelClone));

            return panelClone;
        }

        public int CloseDisplayedPanel(string panelName) => CloseDisplayedPanel((panelName, null), true);
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