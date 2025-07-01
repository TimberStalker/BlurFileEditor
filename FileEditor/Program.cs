using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlurFileFormats.FlaskReflection;
using Editor.Rendering;
using Editor.Windows;
using Editor.Windows.Popups;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Pango;
using static Editor.Rendering.GL;
using ImGuiController = Editor.Rendering.IMGUI.ImGuiController;

namespace Editor
{
    /// <summary>
    /// The entry point of the editor.
    /// </summary>
    static class Program 
    {
        [AllowNull]
        static SynchronizationContext synchronizationContext;
        static GuiWindowManager WindowManager = new();

        public static void ExecuteOnMainThread(Action action) => ExecuteOnMainThread(_ => action(), null);
        public static void ExecuteOnMainThread<T1, T2>(Action<T1, T2> action, T1 state1, T2 state2)
            => ExecuteOnMainThread(c => action(c.state1, c.state2), (state1, state2));
        
        public static void ExecuteOnMainThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 state1, T2 state2, T3 state3)
            => ExecuteOnMainThread(c => action(c.state1, c.state2, c.state3), (state1, state2, state3));

        public static void ExecuteOnMainThread<T>(Action<T> action, T state)
        {
            if(SynchronizationContext.Current != synchronizationContext)
            {
                synchronizationContext.Post(static s =>
                {
                    (Action<T> action, T state) = ((Action<T>, T))s!;
                    action(state);
                }, (action, state));
            }
            else
            {
                action(state);
            }
        }
        public static void ExecuteOnMainThread(Action<object?> action, object state)
        {
            if(SynchronizationContext.Current != synchronizationContext)
            {
                synchronizationContext.Post(static s =>
                {
                    (Action<object?> action, object state) = ((Action<object?>, object))s!;
                    action(state);
                }, (action, state));
            }
            else
            {
                action(state);
            }
        }
        static MainWindow mainGuiWindow = new();
        static void Main(string[] _args) {
            SynchronizationContext.SetSynchronizationContext(synchronizationContext = new SynchronizationContext());

            WindowCreationProps _winProps = new WindowCreationProps() {
                Title = "BLUR FILE EDITOR",
                IsResizable = true,
            };

            Window _window = new Window(_winProps);
            new ImGuiController();
            ImGui.CreateContext();

            _window.OnUpdate += static () => {
                mainGuiWindow.Draw(Window.Instance.WindowSize);
                ImGui.ShowDemoWindow();
            };

            _window.Loop();
        }
    }
}

class UndoCommandListBuffer : List<UndoCommand>, ICommandBuffer
{
    public void Add<T>(in T value, Action<T> redo, Action<T> undo) where T : notnull
    {
        Add(UndoCommand.Create(value, redo, undo));
    }
}
public interface ICommandBuffer
{
    void Add<T>(in T value, Action<T> redo, Action<T> undo) where T : notnull;
}

public static class CommandBufferExtensions
{
    record struct ValueChanged<TTarget, TValue>(TTarget target, TValue oldValue, TValue newValue, Action<TTarget, TValue> action);
    public static void Add<TTarget, TValue>(this ICommandBuffer commandBuffer, TTarget target, TValue newValue, TValue oldValue, Action<TTarget, TValue> action)
    {
        commandBuffer.Add(new ValueChanged<TTarget, TValue>(target, oldValue, newValue, action),
            static v => v.action(v.target, v.newValue),
            static v => v.action(v.target, v.oldValue));
    }
}
