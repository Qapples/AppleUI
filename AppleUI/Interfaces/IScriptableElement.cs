using AppleUI.Interfaces.Behavior;

namespace AppleUI.Interfaces
{
    public interface IScriptableElement
    { 
        /// <summary>
        /// A list of user-defined scripts that determine additional behavior of this UI element.
        /// <see cref="IElementBehaviorScript.Update"/> is ran every frame the UI element is active. Scripts not
        /// included in this array will NOT be updated.
        /// </summary>
        IElementBehaviorScript[] Scripts { get; }
        
        /// <summary>
        /// Uses the <see cref="UserInterfaceManager.ScriptAssembly"/> from an <see cref="UserInterfaceManager"/> to
        /// load the scripts into the UI element. <see cref="ElementScriptInfo"/> instances stored within the object are
        /// used to create the scripts.
        /// </summary>
        void LoadScripts(UserInterfaceManager manager);
    }

    /// <summary>
    /// Provides additional helper functions for <see cref="IScriptableElement"/> instances.
    /// </summary>
    public static class ScriptableElementExtensions
    {
        /// <summary>
        /// Initializes all scripts associated with a <see cref="UserInterfaceElement"/> that has implemented
        /// <see cref="IScriptableElement"/>.
        /// </summary>
        /// <remarks>This method is NOT recursive.</remarks>
        /// <param name="scriptableElement">The user interface element containing the scripts to initialize.</param>
        public static void InitScripts(this IScriptableElement scriptableElement)
        {
            foreach (IElementBehaviorScript script in scriptableElement.Scripts)
            {
                script.Init((UserInterfaceElement) scriptableElement);
            }
        }
    }
}