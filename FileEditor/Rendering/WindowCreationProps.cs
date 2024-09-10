using System;

namespace Editor.Rendering {

    /// <summary>
    /// Contain the creation props given to create the <see cref="Window"/> instance.
    /// </summary>
    public struct WindowCreationProps {
        
        /// <summary>
        /// The title of the window.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The default width of the window.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The default height of the window.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Enable V-Sync?
        /// </summary>
        public bool SwapInterval { get; set; }
        /// <summary>
        /// Will the window able to be resized by the user?
        /// </summary>
        public bool IsResizable { get; set; }
        /// <summary>
        /// Should the window the maximized?
        /// </summary>
        public bool IsMaximized { get; set; }

        /// <summary>
        /// Create a new <see cref="WindowCreationProps"/> with default values.
        /// </summary>
        public WindowCreationProps() {
            Title = "NO TITLE";

            Width = 1270;
            Height = 720;

            SwapInterval = true;
            IsResizable = true;
            IsMaximized = false;
        }
        
    }
    
}