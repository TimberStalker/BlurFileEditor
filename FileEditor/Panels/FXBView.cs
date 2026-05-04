using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Formats.Asn1;
using System.Reflection.Metadata;
using System.Text;
using BlurFileFormats.Models;
using BlurFileFormats.Shaders;
using Editor.Views;
using GLib;
using Hexa.NET.ImGui;
using Hexa.NET.OpenGL;
using HlslDecompiler;
using HlslDecompiler.DirectXShaderModel;
using HlslDecompiler.Hlsl;

namespace Editor.Panels;

internal class FXBView : IDynamicView
{
    public FXB FXB { get; }
    public string FilePath { get; }

    public string Name => Path.GetFileName(FilePath);
    string IDynamicView.Id => FilePath;
    string? IDynamicView.Shortcut => null;

    object? selected;
    Dictionary<Shader, string> texts = new();
    public FXBView(GL gl, FXB fxb, string filePath)
    {
        FXB = fxb;
        FilePath = filePath;
        foreach (var technique in FXB.techniques)
        {
            foreach (var pass in technique.passes)
            {
                try
                {
                    texts[pass.vertexShader] = DecompileShader(pass.vertexShader.bytecode);
                    var shader = gl.CreateShader(GLShaderType.VertexShader);
                    gl.ShaderSource(shader, texts[pass.vertexShader]);
                    gl.CompileShader(shader);

                    var infoLog = gl.GetShaderInfoLog(shader);

                    gl.DeleteShader(shader);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Vertex Shader for {technique.name}:{pass.name} failed to decompile: {ex}");
                    texts[pass.vertexShader] = "Failed to Decompile";
                }
                try
                {
                    texts[pass.pixelShader] = DecompileShader(pass.pixelShader.bytecode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Pixel Shader for {technique.name}:{pass.name} failed to decompile: {ex}");
                    texts[pass.pixelShader] = "Failed to Decompile";
                }
            }
        }
    }
    public static string DecompileShader(byte[] bytes)
    {
        using var input = new ShaderReader(new MemoryStream(bytes, false), false);
        ShaderModel shader = input.ReadShader();

        var hlslWriter = new GlslWriter(shader);

        using var writer = new StringWriter();
        hlslWriter.Write(writer);
        return writer.ToString();
    }

    public bool Draw(GL gl)
    {
        bool open = true;
        if (ImGui.Begin($"{Path.GetFileName(FilePath)}##{FilePath}", ref open, ImGuiWindowFlags.NoCollapse)) {
            var tableSize = ImGui.GetContentRegionAvail();
            if(ImGui.BeginTable("DataTable", 2, ImGuiTableFlags.Resizable, tableSize))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (ImGui.BeginChild("left"))
                {
                    ImGuiTreeNodeFlags techniquesFlags = ImGuiTreeNodeFlags.None;
                    if (FXB.techniques.Count == 0) techniquesFlags |= ImGuiTreeNodeFlags.Leaf;
                    if (ImGui.TreeNodeEx("Techniques", techniquesFlags))
                    {
                        foreach (var technique in FXB.techniques)
                        {
                            ImGuiTreeNodeFlags techniqueFlags = ImGuiTreeNodeFlags.OpenOnArrow;
                            if (technique.passes.Count == 0) techniqueFlags |= ImGuiTreeNodeFlags.Leaf;
                            if (selected == technique) techniqueFlags |= ImGuiTreeNodeFlags.Selected;
                            var techniqueNodeOpen = ImGui.TreeNodeEx(technique.name, techniqueFlags);
                            if(ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                            {
                                selected = technique;
                            }
                            if (techniqueNodeOpen)
                            {
                                foreach (var pass in technique.passes)
                                {
                                    ImGuiTreeNodeFlags passFlags = ImGuiTreeNodeFlags.OpenOnArrow;
                                    if (selected == pass) passFlags |= ImGuiTreeNodeFlags.Selected;
                                    var passNodeOpen = ImGui.TreeNodeEx(pass.name, passFlags);
                                    if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                                    {
                                        selected = pass;
                                    }
                                    if (passNodeOpen)
                                    {
                                        ImGuiTreeNodeFlags vertexFlags = ImGuiTreeNodeFlags.OpenOnArrow;
                                        if (selected == pass.vertexShader) vertexFlags |= ImGuiTreeNodeFlags.Selected;
                                        var vertexNodeOpen = ImGui.TreeNodeEx("Vertex", vertexFlags);
                                        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                                        {
                                            selected = pass.vertexShader;
                                        }
                                        if (vertexNodeOpen)
                                        {

                                            for (int i = 0; i < pass.vertexShader.bindings.Length; i++)
                                            {
                                                FunctionBinding binding = pass.vertexShader.bindings[i];
                                                if(ImGui.TreeNodeEx($"[{i}] {binding.name} (Param{binding.paramaterIndex}:T{binding.type}) {binding.unknown}:{binding.count}", ImGuiTreeNodeFlags.Leaf))
                                                {
                                                    ImGui.TreePop();
                                                }
                                            }
                                            ImGui.TreePop();
                                        }

                                        ImGuiTreeNodeFlags pixelFlags = ImGuiTreeNodeFlags.OpenOnArrow;
                                        if (selected == pass.pixelShader) pixelFlags |= ImGuiTreeNodeFlags.Selected;
                                        var pixelNodeOpen = ImGui.TreeNodeEx("Pixel", pixelFlags);
                                        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                                        {
                                            selected = pass.pixelShader;
                                        }
                                        if (pixelNodeOpen)
                                        {
                                            for (int i = 0; i < pass.pixelShader.bindings.Length; i++)
                                            {
                                                FunctionBinding binding = pass.pixelShader.bindings[i];
                                                if(ImGui.TreeNodeEx($"[{i}] {binding.name} (Param{binding.paramaterIndex}:T{binding.type}) {binding.unknown}:{binding.count}", ImGuiTreeNodeFlags.Leaf))
                                                {
                                                    ImGui.TreePop();
                                                }
                                            }
                                            ImGui.TreePop();
                                        }

                                        ImGui.TreePop();
                                    }
                                }
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }
                
                    ImGuiTreeNodeFlags parameterFlags = ImGuiTreeNodeFlags.None;
                    if (FXB.parameters.Count == 0) parameterFlags |= ImGuiTreeNodeFlags.Leaf;
                    if (ImGui.TreeNodeEx("Parameters", parameterFlags))
                    {
                        foreach (var parameter in FXB.parameters)
                        {
                            ImGuiTreeNodeFlags techniqueFlags = ImGuiTreeNodeFlags.Leaf;
                            if (selected == parameter) techniqueFlags |= ImGuiTreeNodeFlags.Selected;
                            var parameterNodeOpen = ImGui.TreeNodeEx(parameter.name, techniqueFlags);
                            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                            {
                                selected = parameter;
                            }
                            if (parameterNodeOpen)
                            {
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }

                    ImGuiTreeNodeFlags declarationFlags = ImGuiTreeNodeFlags.None;
                    if (FXB.inputDeclarations.Count == 0) declarationFlags |= ImGuiTreeNodeFlags.Leaf;
                    if (ImGui.TreeNodeEx("Input Declarations", declarationFlags))
                    {
                        foreach (var inputDeclaration in FXB.inputDeclarations)
                        {
                            ImGuiTreeNodeFlags inputDecFlags = ImGuiTreeNodeFlags.Leaf;
                            if (selected == inputDeclaration) inputDecFlags |= ImGuiTreeNodeFlags.Selected;
                            var declarationNodeOpen = ImGui.TreeNodeEx($"{inputDeclaration.usage}:{inputDeclaration.usageIndex}", inputDecFlags);
                            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                            {
                                selected = inputDeclaration;
                            }
                            if (declarationNodeOpen)
                            {
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }

                    ImGuiTreeNodeFlags structsFlags = ImGuiTreeNodeFlags.None;
                    if (FXB.structs.Count == 0) structsFlags |= ImGuiTreeNodeFlags.Leaf;
                    if (ImGui.TreeNodeEx("Structs", structsFlags))
                    {
                        foreach (var fxStruct in FXB.structs)
                        {
                            ImGuiTreeNodeFlags structFlags = ImGuiTreeNodeFlags.OpenOnArrow;
                            if (fxStruct.members.Count == 0) structFlags |= ImGuiTreeNodeFlags.Leaf;
                            if (selected == fxStruct) structFlags |= ImGuiTreeNodeFlags.Selected;
                            var structNodeOpen = ImGui.TreeNodeEx($"{fxStruct.name}##{fxStruct.GetHashCode()}", structFlags);
                            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                            {
                                selected = fxStruct;
                            }
                            if (structNodeOpen)
                            {
                                foreach (var member in fxStruct.members)
                                {
                                    ImGuiTreeNodeFlags memberFlags = ImGuiTreeNodeFlags.Leaf;
                                    if (selected == member) memberFlags |= ImGuiTreeNodeFlags.Selected;
                                    var memberNodeOpen = ImGui.TreeNodeEx($"{member.name}##{member.GetHashCode()}", memberFlags);
                                    if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                                    {
                                        selected = member;
                                    }
                                    if (memberNodeOpen)
                                    {
                                        ImGui.TreePop();
                                    }
                                }
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }
                    ImGui.EndChild();
                }

                ImGui.TableSetColumnIndex(1);
                if (ImGui.BeginChild("right"))
                {
                    switch (selected)
                    {
                        case Technique technique:
                            ImGui.LabelText("Name", technique.name);
                            DrawAnnotations(technique.annotations);
                            break;
                        case Pass pass:
                            ImGui.LabelText("Name", pass.name);
                            DrawAnnotations(pass.annotations);
                            break;
                        case Shader shader:
                            ImGui.TextWrapped(texts[shader]);
                            break;
                        case FXParameter parameter:
                            ImGui.LabelText("Name", parameter.name);
                            ImGui.LabelText("Type", parameter.parameterType.ToString());
                            ImGui.LabelText("Semantic", parameter.semantic.ToString());
                            ImGui.LabelText("Count", parameter.count.ToString());
                            ImGui.LabelText("Offset", parameter.offset.ToString());
                            ImGui.LabelText("Unk", parameter.unknownByte.ToString());
                            DrawAnnotations(parameter.annotations);
                            break;
                        case InputDeclaration inputDeclaration:
                            ImGui.LabelText("Usage", inputDeclaration.usage.ToString());
                            ImGui.InputInt("Usage Index", ref inputDeclaration.usageIndex);
                            DrawAnnotations(inputDeclaration.annotations);
                            break;
                        case FXStruct fxStruct:
                            ImGui.LabelText("Name", fxStruct.name);
                            break;
                        case FXStructMember fxStructMember:
                            ImGui.LabelText("Name", fxStructMember.name);
                            ImGui.InputInt("Size", ref fxStructMember.size);
                            DrawAnnotations(fxStructMember.annotations);
                            break;
                    }
                    ImGui.EndChild();
                }
                ImGui.EndTable();
            }
        }
        ImGui.End();
        return open;
    }

    private void DrawAnnotations(IEnumerable<Annotation> annotations)
    {
        ImGui.Text("Annotations");
        foreach (var annotation in annotations)
        {
            DrawAnnotation(annotation);
        }
    }

    public void DrawAnnotation(Annotation annotation)
    {
        switch (annotation.value)
        {
            case BoolAnnotation v:
            {
                bool value = v.value;
                ImGui.Checkbox(annotation.name, ref value);
                break;
            }
            case IntAnnotation v:
            {
                int value = v.value;
                ImGui.InputInt(annotation.name, ref value);
                break;
            }
            case StringAnnotation v:
            {
                string value = v.value;
                ImGui.LabelText(annotation.name, value);
                break;
            }
        }
    }
    //public void DrawTreeNode(string name, )
}
