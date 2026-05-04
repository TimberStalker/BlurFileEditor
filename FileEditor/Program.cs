using System.Numerics;
using System.Runtime.CompilerServices;
using Editor.Windows;
using Gdk;
using Hexa.NET.GLFW;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.GLFW;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Utilities;
using Hexa.NET.OpenGL;
using HexaGen.Runtime;
using static OpenGL;
using GLFWmonitorPtr = Hexa.NET.GLFW.GLFWmonitorPtr;
using GLFWwindowPtr = Hexa.NET.GLFW.GLFWwindowPtr;

NativeCallback<GLFWerrorfun> error;
unsafe
{
    error = new(static (errorCode, desciption) =>
    {
        Console.WriteLine(Utils.DecodeStringUTF8(desciption));
    });
    GLFW.SetErrorCallback(error);
}

GLFW.Init();
string glslVersion = "#version 150";
GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
GLFW.WindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 2);
GLFW.WindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);  // 3.2+ only
unsafe
{
    GLFW.SetErrorCallback((type, message) =>
    {
        Console.WriteLine($"GLFW Error {type}: {Utils.DecodeStringUTF8(message)}");
    });
}
var mon = GLFW.GetPrimaryMonitor();
float mainScale = ImGuiImplGLFW.GetContentScaleForMonitor(Unsafe.BitCast<GLFWmonitorPtr, Hexa.NET.ImGui.Backends.GLFW.GLFWmonitorPtr>(mon));
GLFWwindowPtr window = GLFW.CreateWindow((int)(1280 * mainScale), (int)(800 * mainScale), "Blur File Editor", null, null);
if (window.IsNull)
{
    Console.WriteLine("Failed to create GLFW window.");
    GLFW.Terminate();
    return;
}

GLFW.MakeContextCurrent(window);

var guiContext = ImGui.CreateContext();
ImGui.SetCurrentContext(guiContext);

// Setup ImGui config.
var io = ImGui.GetIO();
io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;     // Enable Keyboard Controls
io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls
io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;         // Enable Docking
io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;       // Enable Multi-Viewport / Platform Windows

ImGui.StyleColorsLight();
var style = ImGui.GetStyle();
style.ScaleAllSizes(mainScale);
style.FontScaleDpi = mainScale;
io.ConfigDpiScaleFonts = true;
io.ConfigDpiScaleViewports = true;

ImGuiFontBuilder builder = new(ImGui.GetIO().Fonts);

builder.AddFontFromFileTTF(Path.Combine("Resources", "Fonts", "Roboto-Regular.ttf"), 16);
builder.SetOption(o =>
{
    //o.MergeMode = true;
    o.GlyphOffset = new Vector2(0, 2);
});
//builder.AddFontFromFileTTF(Path.Combine("Resources", "Fonts", "MaterialSymbolsRounded.ttf"), 16.0f, [0xe003, 0xF8FF]);

builder.Build();

if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
{
    style.WindowRounding = 0.0f;
    style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
}

ImGuiImplGLFW.SetCurrentContext(guiContext);

if (!ImGuiImplGLFW.InitForOpenGL(Unsafe.BitCast<GLFWwindowPtr, Hexa.NET.ImGui.Backends.GLFW.GLFWwindowPtr>(window), true))
{
    Console.WriteLine("Failed to init ImGui Impl GLFW");
    GLFW.Terminate();
    return;
}

ImGuiImplOpenGL3.SetCurrentContext(guiContext);
if (!ImGuiImplOpenGL3.Init(glslVersion))
{
    Console.WriteLine("Failed to init ImGui Impl OpenGL3");
    GLFW.Terminate();
    return;
}

SetOpenGL(new(new BindingsContext(window)));
MainWindow mainGuiWindow = new(GL3);
// Main loop
while (GLFW.WindowShouldClose(window) == 0)
{
    // Poll for and process events
    GLFW.PollEvents();

    if (GLFW.GetWindowAttrib(window, GLFW.GLFW_ICONIFIED) != 0)
    {
        ImGuiImplGLFW.Sleep(10);
        continue;
    }

    GLFW.MakeContextCurrent(window);
    GL3.ClearColor(1, 0.8f, 0.75f, 1);
    GL3.Clear(GLClearBufferMask.ColorBufferBit);

    ImGuiImplOpenGL3.NewFrame();
    ImGuiImplGLFW.NewFrame();
    ImGui.NewFrame();

    style.FrameRounding = 4;
    style.WindowRounding = 4;
    style.ChildRounding = 4;
    style.PopupRounding = 4;
    style.FrameBorderSize = 1;
    //style.GrabRounding = 4;
    style.ScrollbarRounding = 4;

    int windowWidth = 0;
    int windowHeight = 0;
    GLFW.GetWindowSize(window, ref windowWidth, ref windowHeight);

    int windowPosX = 0;
    int windowPosY = 0;
    GLFW.GetWindowPos(window, ref windowPosX, ref windowPosY);

    ImGui.PushFont(io.Fonts.Fonts[0], 0);
    {
        ImGui.SetNextWindowPos(new Vector2(windowPosX, windowPosY), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(windowWidth, windowHeight), ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.Begin("DockSpace Example", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                                       ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                       ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus);
        {
            ImGui.PopStyleVar();

            //mainGuiWindow.DrawMainMenu();

            ImGui.DockSpace(ImGui.GetID("DockSpace"));
            //ImGui.SetNextWindowPos(new Vector2(windowPosX, windowPosY));
            mainGuiWindow.Draw(new Vector2(windowWidth, windowHeight));
            //ImGui.ShowDemoWindow();
        }
        ImGui.End();
        //var pos = ImGui.GetWindowPos();
        //var size = ImGui.GetWindowSize();
        //(windowPosX, windowPosY) = ((int)pos.X, (int)pos.Y);
        //(windowWidth, windowHeight) = ((int)size.X, (int)size.Y);
    }
    ImGui.PopFont();

    ImGui.Render();

    GLFW.MakeContextCurrent(window);
    ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

    if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
    {
        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();
    }

    GLFW.MakeContextCurrent(window);
    GLFW.SwapBuffers(window);
}

ImGuiImplOpenGL3.Shutdown();
ImGuiImplOpenGL3.SetCurrentContext(null);
ImGuiImplGLFW.Shutdown();
ImGuiImplGLFW.SetCurrentContext(null);
ImGui.DestroyContext();
GL3.Dispose();

// Clean up and terminate GLFW
GLFW.DestroyWindow(window);
GLFW.Terminate();

public static class OpenGL
{
    public static Hexa.NET.OpenGL.GL GL3 { get; private set; } = null!;

    public static void SetOpenGL(Hexa.NET.OpenGL.GL gl) => GL3 = gl;
}

internal unsafe class BindingsContext : HexaGen.Runtime.IGLContext
{
    private GLFWwindowPtr window;

    public BindingsContext(GLFWwindowPtr window)
    {
        this.window = window;
    }

    public nint Handle => (nint)window.Handle;

    public bool IsCurrent => GLFW.GetCurrentContext() == window;

    public void Dispose()
    {
    }

    public nint GetProcAddress(string procName)
    {
        return (nint)GLFW.GetProcAddress(procName);
    }

    public bool IsExtensionSupported(string extensionName)
    {
        return GLFW.ExtensionSupported(extensionName) != 0;
    }

    public void MakeCurrent()
    {
        GLFW.MakeContextCurrent(window);
    }

    public void SwapBuffers()
    {
        GLFW.SwapBuffers(window);
    }

    public void SwapInterval(int interval)
    {
        GLFW.SwapInterval(interval);
    }

    public bool TryGetProcAddress(string procName, out nint procAddress)
    {
        procAddress = (nint)GLFW.GetProcAddress(procName);
        return procAddress != 0;
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
