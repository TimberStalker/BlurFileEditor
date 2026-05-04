using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using BlurFileFormats.Models;
using Editor.Projects;
using Editor.Views;
using FFmpeg.AutoGen;
using Gdk;
using Hexa.NET.ImGui;
using Hexa.NET.OpenGL;
using Hexa.NET.OpenGL.ARB;
using Vortice.Dxc;
using static System.Collections.Specialized.BitVector32;
using static GLib.Signal;

namespace Editor.Panels;

public class ModelView : IDynamicView
{
    public CPModel Model { get; }
    public ModelRenderInfo RenderInfo { get; }
    public string FilePath { get; }

    public string Name => Path.GetFileName(FilePath);
    string IDynamicView.Id => FilePath;
    string? IDynamicView.Shortcut => null;
    List<ModelTexture> modelTextures = [];
    
    object? selectedItem;
    object? viewportSelection;

    uint fbo;
    uint fboColor;
    uint fboDepth;

    int lod;

    Vector2 fboSize;

    Vector3 position = new Vector3(MathF.PI, 0, -15);
    Vector3 sunDirection = Vector3.Normalize(new Vector3(1, 1, 0));
    Vector3 ambientColor = new Vector3(0.1f, 0.1f, 0.1f);
    Vector3 normalColor = new Vector3(0.6f, 0.6f, 0.6f);
    Vector3 selectedColor = new Vector3(0.7f, 0.7f, 0.9f);
    Vector3 lineColor = new Vector3(0.3f, 0.3f, 0.5f);

    public void Execute(){}
    public unsafe ModelView(GL gl, CPModel model, Project activeProject, string filePath)
    {
        var lua = new NLua.Lua();

        Model = model;
        FilePath = filePath;
        modelTextures.Capacity = model.Scene.textures.Length;
        RenderInfo = ModelRenderInfo.Create(model, activeProject, gl);
        foreach (var item in model.Scene.textures)
        {
            modelTextures.Add(ModelTexture.Create(gl, item));
        }
        foreach (var resourceBlock in model.Scene.resourceBlocks)
        {
            foreach (var lod in resourceBlock.lods)
            {
                foreach (var tex in lod.textures)
                {
                    if(tex is DXTTexture texture)
                    {
                        modelTextures.Add(ModelTexture.Create(gl, texture));
                    }
                }
            }
        }


        fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, fbo);

        fboColor = gl.GenTexture();
        gl.BindTexture(GLTextureTarget.Texture2D, fboColor);
        gl.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba, 0, 0, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedByte, null);
        gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)GLTextureMinFilter.Linear);
        gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)GLTextureMagFilter.Linear);

        gl.FramebufferTexture2D(GLFramebufferTarget.Framebuffer, GLFramebufferAttachment.ColorAttachment0, GLTextureTarget.Texture2D, fboColor, 0);

        fboDepth = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GLRenderbufferTarget.Renderbuffer, fboDepth);
        gl.RenderbufferStorage(GLRenderbufferTarget.Renderbuffer, GLInternalFormat.DepthComponent24, 0, 0);
        gl.FramebufferRenderbuffer(GLFramebufferTarget.Framebuffer, GLFramebufferAttachment.DepthAttachment, GLRenderbufferTarget.Renderbuffer, fboDepth);

        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);
    }
    bool drawInspector;
    public unsafe bool Draw(GL gl)
    {
        bool open = true;
        if (ImGui.Begin($"{Path.GetFileName(FilePath)}##{FilePath}", ref open, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Checkbox("Inspector", ref drawInspector);
            if(drawInspector)
            {
                DrawInspector();
            }
            else
            {
                if (ImGui.BeginTable("ModelTab", 2, ImGuiTableFlags.Resizable))
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    DrawModel(gl);
                    if(ImGui.IsItemHovered())
                    {
                        var io = ImGui.GetIO();
                        if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
                        {
                            var delta = io.MouseDelta * 0.01f;
                            position += new Vector3(delta.X, delta.Y, 0);
                        }
                        position.Z += io.MouseWheel * 0.7f;
                    }
                    ImGui.TableSetColumnIndex(1);
                    if (ImGui.BeginChild("Heierarchy", ImGuiChildFlags.ResizeY))
                    {
                        ImGui.InputInt("LOD", ref lod);
                        if (ImGui.TreeNodeEx("Models"))
                        {
                            for (int i = 0; i < RenderInfo.models.Count; i++)
                            {
                                var model = RenderInfo.models[i];
                                if (model.parent is not null) continue;
                                
                                DrawRenderModelHeierachy(model);
                            }
                            ImGui.TreePop();
                        }
                        if (ImGui.TreeNodeEx("Materials"))
                        {
                            for (int i = 0; i < RenderInfo.materials.Count; i++)
                            {
                                var material = RenderInfo.materials[i];
                                var shader = material.shader;
                                if (ImGui.TreeNodeEx($"{shader.name}##{material.GetHashCode()}"))
                                {
                                    ImGui.Text(shader.name);
                                    DrawList("Parameters", shader.parameters, (i, p) =>
                                    {
                                        if(ImGui.TreeNodeEx($"{p.parameterType}{p.count} {p.name}"))
                                        {

                                            ImGui.TreePop();
                                        }
                                    });
                                    DrawList("Inputs", shader.inputDeclarations, (i, d) =>
                                    {
                                        if(ImGui.TreeNodeEx($"{d.usage}{d.usageIndex}"))
                                        {

                                            ImGui.TreePop();
                                        }
                                    });
                                    ImGui.TreePop();
                                }
                            }
                            ImGui.TreePop();
                        }
                        ImGui.EndChild();
                    }
                    if (ImGui.BeginChild("ViewportInspector"))
                    {
                        ImGui.Text("Inspector");
                        switch (viewportSelection)
                        {
                            case RenderModel model:
                                ImGui.InputText("Name", ref model.name, 255);
                                InputMatrix4x4(ref model.transform);
                                break;
                            case RenderElement element:
                                ImGui.InputText("Name", ref element.name, 255);
                                InputMatrix4x4(ref element.transform);
                                var available = ImGui.GetContentRegionAvail();

                                if(ImGui.BeginListBox("##Meshes", new Vector2(available.X, 80)))
                                {
                                    for (int i = 0; i < element.mesh.subMeshes.Count; i++)
                                    {
                                        if(ImGui.TreeNodeEx($"Mesh{i}"))
                                        {
                                            ImGui.TreePop();
                                        }
                                    }
                                    ImGui.EndListBox();
                                }
                                break;
                            default:
                                break;
                        }
                        ImGui.EndChild();
                    }
                    ImGui.EndTable();
                }
            }
            
        }
        ImGui.End();
        return open;
    }
    private void DrawRenderModelHeierachy(RenderModel renderModel)
    {
        var flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.DrawLinesToNodes | ImGuiTreeNodeFlags.OpenOnArrow;
        if (renderModel.elements.Count == 0 && renderModel.children.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (viewportSelection == renderModel) flags |= ImGuiTreeNodeFlags.Selected;
        bool drawNode = ImGui.TreeNodeEx($"##{renderModel.GetHashCode()}", flags);

        if(ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            viewportSelection = renderModel;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##Display{renderModel.GetHashCode()}", ref renderModel.draw);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(renderModel.name);
        if (drawNode)
        {
            //InputMatrix4x4(ref renderModel.transform);
            foreach(var childModel in renderModel.children)
            {
                DrawRenderModelHeierachy(childModel);
            }
            foreach (var element in renderModel.elements)
            {
                if (element.parentElement is not null) continue;
                DrawRenderElementInspector(element);
            }
            ImGui.TreePop();
        }
    }
    private void DrawRenderElementInspector(RenderElement renderElement)
    {
        var flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.DrawLinesToNodes | ImGuiTreeNodeFlags.OpenOnArrow;
        if (renderElement.children.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (viewportSelection == renderElement) flags |= ImGuiTreeNodeFlags.Selected;
        bool drawNode = ImGui.TreeNodeEx($"##{renderElement.GetHashCode()}", flags);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            viewportSelection = renderElement;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##Display{renderElement.GetHashCode()}", ref renderElement.draw);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(renderElement.name);
        if (drawNode)
        {
            //InputMatrix4x4(ref renderModel.transform);
            foreach (var element in renderElement.children)
            {
                DrawRenderElementInspector(element);
            }
            ImGui.TreePop();
        }
    }
    enum SelectionLevel
    {
        None,
        Secondary,
        Primary
    }
    private unsafe void DrawModel(GL gl)
    {
        var availableRegion = ImGui.GetContentRegionAvail();
        if (availableRegion == Vector2.Zero) return;
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, fbo);
        GlCheckError(gl);
        if(availableRegion.X > fboSize.X || availableRegion.Y > fboSize.Y || availableRegion.X < fboSize.X/2 || availableRegion.Y < fboSize.Y/2)
        {

            int x = (int)(availableRegion.X * 1.5f);
            int y = (int)(availableRegion.Y * 1.5f);
            fboSize = new Vector2(x, y);

            gl.BindTexture(GLTextureTarget.Texture2D, fboColor);
            GlCheckError(gl);
            gl.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba, x, y, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedByte, null);
            GlCheckError(gl);
            gl.BindRenderbuffer(GLRenderbufferTarget.Renderbuffer, fboDepth);
            GlCheckError(gl);
            gl.RenderbufferStorage(GLRenderbufferTarget.Renderbuffer, GLInternalFormat.DepthComponent24, x, y);
            GlCheckError(gl);
            GlCheckError(gl);
            Console.WriteLine("Resized Texture Target");
        }
        gl.Viewport(0, 0, (int)availableRegion.X, (int)availableRegion.Y);
        gl.Enable(GLEnableCap.DepthTest);
        GlCheckError(gl);
        gl.CullFace(GLTriangleFace.FrontAndBack);
        gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(GLClearBufferMask.ColorBufferBit | GLClearBufferMask.DepthBufferBit);
        GlCheckError(gl);
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(MathF.PI / 4, availableRegion.X / availableRegion.Y, 0.1f, 100);
        Matrix4x4.Invert(Matrix4x4.CreateTranslation(0, 0, position.Z) * Matrix4x4.CreateFromYawPitchRoll(position.X, position.Y, 0), out var view);
        for(int i = 0; i < RenderInfo.models.Count; i++)
        {
            var modelMatrix = Matrix4x4.Identity;
            RenderRenderModel(RenderInfo.models[i], lod, SelectionLevel.None, ref projection, ref view, ref modelMatrix, gl);
            var element = RenderInfo.elements[i];
            if (!element.draw) continue;
        }
        gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);
        GlCheckError(gl);
        var ratio = availableRegion / fboSize;
        ImGui.Image(new ImTextureRef(texId: (ImTextureID)fboColor), availableRegion, new Vector2(0, ratio.Y), new Vector2(ratio.X, 0));
    }
    private void RenderRenderModel(RenderModel renderModel, int lod, SelectionLevel parentSelection, ref Matrix4x4 projection, ref Matrix4x4 view, ref Matrix4x4 model, GL gl)
    {
        if (!renderModel.draw) return;
        var mulMat = model * renderModel.transform;
        var selection = parentSelection;
        if (renderModel == viewportSelection) selection = SelectionLevel.Primary;
        else if(selection == SelectionLevel.Primary) selection = SelectionLevel.Secondary;
        foreach (var element in renderModel.elements)
        {
            if (element.parentElement is not null) continue;
            RenderRenderElement(element, lod, selection, ref projection, ref view, ref mulMat, gl);
        }
        foreach (var childModel in renderModel.children)
        {
            RenderRenderModel(childModel, lod, selection, ref projection, ref view, ref mulMat, gl);
        }
    }
    private void RenderRenderElement(RenderElement renderElement, int lod, SelectionLevel parentSelection, ref Matrix4x4 projection, ref Matrix4x4 view, ref Matrix4x4 model, GL gl)
    {
        if (!renderElement.draw) return;
        var mulMat = model * renderElement.transform;

        var selection = parentSelection;
        if (renderElement == viewportSelection) selection = SelectionLevel.Primary;
        else if (selection == SelectionLevel.Primary) selection = SelectionLevel.Secondary;

        DrawMesh(renderElement.mesh, lod, selection, ref projection, ref view, ref renderElement.transform, gl);
        foreach (var childElement in renderElement.children)
        {
            RenderRenderElement(childElement, lod, selection, ref projection, ref view, ref mulMat, gl);
        }
    }
    private unsafe void DrawMesh(Mesh mesh, int lod, SelectionLevel selection, ref Matrix4x4 projection, ref Matrix4x4 view, ref Matrix4x4 model, GL gl)
    {
        foreach (var subMesh in mesh.subMeshes)
        {
            if (subMesh.lods.Count == 0) continue;

            gl.UseProgram(subMesh.shader);
            GlCheckError(gl);
            gl.UniformMatrix4fv(gl.GetUniformLocation(subMesh.shader, "projection"), 1, false, MemoryMarshal.CreateSpan(ref projection.M11, 16));
            gl.UniformMatrix4fv(gl.GetUniformLocation(subMesh.shader, "view"), 1, false, MemoryMarshal.CreateSpan(ref view.M11, 16));
            gl.UniformMatrix4fv(gl.GetUniformLocation(subMesh.shader, "model"), 1, false, MemoryMarshal.CreateSpan(ref model.M11, 16));

            gl.Uniform1i(gl.GetUniformLocation(subMesh.shader, "selection"), -1);
            gl.Uniform3fv(gl.GetUniformLocation(subMesh.shader, "lightDirection"), 1, MemoryMarshal.CreateSpan(ref sunDirection.X, 3));
            gl.Uniform3fv(gl.GetUniformLocation(subMesh.shader, "ambientColor"), 1, MemoryMarshal.CreateSpan(ref ambientColor.X, 3));

            if(selection != SelectionLevel.None)
            {
                gl.Uniform3fv(gl.GetUniformLocation(subMesh.shader, "diffuseColor"), 1, MemoryMarshal.CreateSpan(ref selectedColor.X, 3));
            }
            else
            {
                gl.Uniform3fv(gl.GetUniformLocation(subMesh.shader, "diffuseColor"), 1, MemoryMarshal.CreateSpan(ref normalColor.X, 3));
            }
            
            gl.Uniform1f(gl.GetUniformLocation(subMesh.shader, "lightStrength"), 1f);

            MeshLod meshlod = subMesh.lods[Math.Clamp(lod, 0, subMesh.lods.Count-1)];
            var primitveType = subMesh.primitiveType switch
            {
                PrimitiveType.TriangleList => GLPrimitiveType.Triangles,
                PrimitiveType.TriangleStrip => GLPrimitiveType.TriangleStrip,
                PrimitiveType.PointList => GLPrimitiveType.Points,
                PrimitiveType.QuadList => GLPrimitiveType.Quads,
                PrimitiveType.QuadStrip => GLPrimitiveType.QuadStrip,
                _ => throw new NotSupportedException($"Primitve type of {subMesh.primitiveType} is not supported")
            };
            gl.BindVertexArray(subMesh.vertexBuffer);
            GlCheckError(gl);
            gl.BindBuffer(GLBufferTargetARB.ElementArrayBuffer, subMesh.indexBuffer);
            GlCheckError(gl);
            gl.DrawElementsBaseVertex(primitveType, meshlod.indexRange.End.Value - meshlod.indexRange.Start.Value, GLDrawElementsType.UnsignedShort, meshlod.indexRange.Start.Value * 2, subMesh.baseVertex);
            if(selection == SelectionLevel.Primary)
            {
                gl.Uniform3fv(gl.GetUniformLocation(subMesh.shader, "diffuseColor"), 1, MemoryMarshal.CreateSpan(ref lineColor.X, 3));
                gl.PolygonMode(GLTriangleFace.FrontAndBack, GLPolygonMode.Line);
                gl.DrawElementsBaseVertex(primitveType, meshlod.indexRange.End.Value - meshlod.indexRange.Start.Value, GLDrawElementsType.UnsignedShort, meshlod.indexRange.Start.Value * 2, subMesh.baseVertex);
                gl.PolygonMode(GLTriangleFace.FrontAndBack, GLPolygonMode.Fill);
            }
            GlCheckError(gl);
        }
    }
    private unsafe void DrawInspector()
    {
        var availableSize = ImGui.GetContentRegionAvail();
        if (ImGui.BeginTable("Inspector", 2, ImGuiTableFlags.Resizable, availableSize))
        {

            ImGui.TableNextRow();
            if (ImGui.TableSetColumnIndex(0))
            {
                if (ImGui.BeginChild("left"))
                {
                    DrawSelectableList("Models", Model.Models, (i, m) => $"[{i}] {m.name}##{m.GetHashCode()}");
                    DrawSelectableList("Elements", Model.Elements, (i, e) => $"[{i}] {e.name}##{e.GetHashCode()}");
                    if (ImGui.TreeNodeEx("Scene"))
                    {
                        var rootNodeFlags = ImGuiTreeNodeFlags.Leaf;
                        if(selectedItem == Model.Scene.rootNode)
                        {
                            rootNodeFlags |= ImGuiTreeNodeFlags.Selected;
                        }
                        bool rootNodeOpen = ImGui.TreeNodeEx($"Root Cull Node {Model.Scene.rootNode.name}", rootNodeFlags);
                        if (ImGui.IsItemClicked())
                        {
                            selectedItem = Model.Scene.rootNode;
                        }
                        if (rootNodeOpen) ImGui.TreePop();
                        DrawSelectableList("Effects", Model.Scene.effects, (i, m) => $"[{i}] {m}");
                        DrawList("Dynamic Material Lists", Model.Scene.dynamicMaterialLists, (i, l) =>
                        {
                            if(l is null)
                            {
                                if(ImGui.TreeNodeEx($"Empty", ImGuiTreeNodeFlags.Leaf))
                                {
                                    ImGui.TreePop();
                                }
                            }
                            else
                            {
                                if(ImGui.TreeNodeEx($"List [{i}]"))
                                {
                                    DrawSelectableList("Types", l.materialTypes, (i, t) => t.name);
                                    DrawSelectableList("Materials", l.materials, (i, t) => $"[{i}] {l.materialTypes[t.typeIndex].name}");
                                    ImGui.TreePop();
                                }
                            }
                        });
                        DrawList("Resource Blocks", Model.Scene.resourceBlocks, (i, b) =>
                        {
                            DrawSelectableParentList(b, _ => $"Block [{i}]", b.lods, (i, l) =>
                            {
                                var lodFlags = ImGuiTreeNodeFlags.None;
                                if (l.vertexBuffers.Length == 0 && l.indexBuffers.Length == 0 && l.renderingNodes.Length == 0 && l.textures.Length == 0)
                                {
                                    lodFlags |= ImGuiTreeNodeFlags.Leaf;
                                }
                                if (ImGui.TreeNodeEx($"Lod [{i}]", lodFlags))
                                {
                                    if (l.vertexBuffers.Length > 0)
                                    {
                                        DrawSelectableList("Vertex Buffers", l.vertexBuffers, (i, b) => $"[{i}] Buffer");
                                    }
                                    if (l.indexBuffers.Length > 0)
                                    {
                                        DrawSelectableList("Index Buffers", l.indexBuffers, (i, b) => $"[{i}] Buffer");
                                    }
                                    if (l.renderingNodes.Length > 0)
                                    {
                                        DrawSelectableList("Nodes", l.renderingNodes, (i, r) => $"[{i}] {r.Type} ({r.name})");
                                    }
                                    if (l.textures.Length > 0)
                                    {
                                        DrawSelectableList("Textures", l.textures, (i, t) => t switch
                                        {
                                            DXTTexture dxt => $"[{i}] dxt: {dxt.name} ({dxt.description.width}x{dxt.description.height})",
                                            LodReferenceTexture lod => $"[{i}] ref: {lod.name} ({lod.index})",
                                            _ => "Empty"
                                        }
                                        );
                                    }
                                    ImGui.TreePop();
                                }
                            });
                        });
                        ImGui.TreePop();
                    }
                    DrawSelectableList("Textures", modelTextures, (i, t) => $"[{i}] {t.textureSpecification.name} ({t.textureSpecification.description.width}x{t.textureSpecification.description.height})");
                    ImGui.EndChild();
                }
            }
            if (ImGui.TableSetColumnIndex(1))
            {
                if (ImGui.BeginChild("right"))
                {
                    switch (selectedItem)
                    {
                        case Model m:

                            ImGui.InputText("Name", ref m.name, (nuint)m.name.Length);
                            ImGui.Text("Bounds");
                            ImGui.InputFloat3("Start", ref m.bounds.start);
                            ImGui.InputFloat3("End", ref m.bounds.end);
                            InputMatrix4x4(ref m.transform);
                            ImGui.InputInt("Model Index", ref m.modelIndex);
                            ImGui.InputInt("Element Count", ref m.elementCount);
                            ImGui.InputInt("Model Data Index", ref m.modelDataIndex);
                            ImGui.InputInt("Parent Index", ref m.parentIndex);
                            ImGui.InputInt("First Child", ref m.firstChild);
                            ImGui.InputInt("Next Sibling", ref m.nextSibling);
                            break;
                        case Element e:
                            ImGui.InputText("Name", ref e.name, (nuint)e.name.Length);
                            ImGui.Text("Bounds");
                            ImGui.InputFloat3("Start", ref e.bounds.start);
                            ImGui.InputFloat3("End", ref e.bounds.end);
                            InputMatrix4x4(ref e.transform);

                            ImGui.InputInt("RenderMeshId", ref e.renderMeshId);
                            ImGui.InputInt("PhysicsShapeId", ref e.physicsShapeId);
                            ImGui.InputInt("ModelIndex", ref e.modelIndex);
                            ImGui.InputInt("ElementIndex", ref e.elementIndex);
                            ImGui.InputInt("ParentElement", ref e.parentElement);
                            ImGui.InputInt("FirstChild", ref e.firstChild);
                            ImGui.InputInt("NextSibling", ref e.nextSibling);
                            break;
                        case CommonRenderListNode n:

                            ImGui.InputText("Name", ref n.name, (nuint)n.name.Length);
                            if (ImGui.TreeNodeEx("Bounds"))
                            {
                                ImGui.InputFloat3("Start", ref n.bounds.start);
                                ImGui.InputFloat3("End", ref n.bounds.end);
                                ImGui.InputFloat("Min", ref n.bounds.distanceMin);
                                ImGui.InputFloat("Max", ref n.bounds.distanceMax);
                                ImGui.TreePop();
                            }
                            ImGui.LabelText("Parent Index", n.parentIndex.ToString());
                            ImGui.LabelText("Location", n.location.ToString());
                            ImGui.InputInt("Group Index", ref n.groupIndex);
                            DrawList("Sub Nodes", n.subNodes, (i, n) =>
                            {
                                ImGui.LabelText("Location", n.ToString());
                            });
                            if(ImGui.TreeNode("Constant Material Types"))
                            {
                                DrawMaterialList(n.constantMaterialTypes);
                                ImGui.TreePop();
                            }
                            DrawMaterials(n.constantMaterialTypes.materialTypes, n.materials);
                            DrawList("Dynamic Material Lists", n.dynamicMaterialLsts, (i, l) =>
                            {
                                if(ImGui.TreeNode($"List [{i}]"))
                                {
                                    DrawMaterialList(l);
                                    ImGui.TreePop();
                                }
                            });
                            DrawList("Primitves", n.primitives, (i, p) =>
                            {
                                DrawPrimitive($"Primitive [{i}]", p);
                            });
                            DrawList("Index Buffer Sources", n.indexBufferSources, (i, s) =>
                            {
                                ImGui.Text($"{s.block}:{s.index}");
                            });
                            DrawList("Vertex Buffer Sources", n.vertexBufferSources, (i, s) =>
                            {
                                ImGui.Text($"{s.block}:{s.index}");
                            });
                            DrawList("Texture Sources", n.textureBufferSources, (i, s) =>
                            {
                                ImGui.Text($"{s.block}:{s.index}");
                            });
                            break;
                        case CullNode c:
                            ImGui.InputText("Name", ref c.name, (nuint)c.name.Length);
                            if(ImGui.TreeNodeEx("Bounds"))
                            {
                                ImGui.InputFloat3("Start", ref c.bounds.start);
                                ImGui.InputFloat3("End", ref c.bounds.end);
                                ImGui.InputFloat("Min", ref c.bounds.distanceMin);
                                ImGui.InputFloat("Max", ref c.bounds.distanceMax);
                                ImGui.TreePop();
                            }
                            ImGui.LabelText("Parent Index", c.parentIndex.ToString());
                            ImGui.LabelText("Location", c.location.ToString());
                            ImGui.InputInt("Group Index", ref c.groupIndex);
                            DrawList("Sub Nodes", c.subNodes, (i, n) =>
                            {
                                ImGui.LabelText("Location", n.ToString());
                            });
                            break;
                        case ModelTexture t:
                            var textureTableavailableRegion = ImGui.GetContentRegionAvail();
                            if (ImGui.BeginTable("TextureTable", 2, ImGuiTableFlags.Resizable, textureTableavailableRegion))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.LabelText("Width", t.textureSpecification.description.width.ToString());
                                ImGui.LabelText("Height", t.textureSpecification.description.height.ToString());
                                ImGui.LabelText("Mipmaps", t.textureSpecification.description.mipmaps.ToString());
                                ImGui.LabelText("Format", Encoding.ASCII.GetString(BitConverter.GetBytes((int)t.textureSpecification.description.textureFormat)));
                                ImGui.LabelText("Usage", t.textureSpecification.description.usage.ToString());
                                ImGui.LabelText("Type", t.textureSpecification.description.type.ToString());
                                DrawSamplerState(t.textureSpecification.samplerState);
                                ImGui.TableSetColumnIndex(1);
                                if (t.texHandle == uint.MaxValue)
                                {
                                    ImGui.Text("Unsupported Texture Format");
                                }
                                else
                                {
                                    var availableRegion = ImGui.GetContentRegionAvail();
                                    var scaleFactorX = availableRegion.X / t.textureSpecification.description.width;
                                    var scaleFactorY = availableRegion.Y / t.textureSpecification.description.height;
                                    var minScaleFactor = Math.Min(scaleFactorX, scaleFactorY);
                                    var textureSize = new Vector2(t.textureSpecification.description.width * minScaleFactor, t.textureSpecification.description.height * minScaleFactor);
                                    ImGui.SetCursorPos(ImGui.GetCursorPos() + (availableRegion - textureSize) / 2);
                                    ImGui.Image(new(texId: (ImTextureID)t.texHandle), textureSize);
                                }
                                ImGui.EndTable();
                            }
                            break;
                        case BlockLod l:

                            DrawList("Vertex Buffers", l.vertexBuffers, (i, v) =>
                            {
                                if (ImGui.TreeNodeEx($"Buffer [{i}]"))
                                {
                                    DrawList("Declaration", v.declaration.elements, (i, d) =>
                                    {
                                        ImGui.Text($"{d.type}:{d.method}:{d.usage} {d.usageIndex}");
                                    });
                                    ImGui.LabelText("Vertex Count", v.vertexCount.ToString());
                                    ImGui.TreePop();
                                }
                            });
                            DrawList("Index Buffers", l.indexBuffers, (i, b) =>
                            {
                                if (ImGui.TreeNodeEx($"Buffer [{i}]"))
                                {
                                    ImGui.LabelText("Index Type", b.indexType.ToString());
                                    ImGui.LabelText("Index Count", b.IndexCount.ToString());
                                    ImGui.TreePop();
                                }
                            });
                            DrawList("Textures", l.textures, (i, t) =>
                            {
                                switch (t)
                                {
                                    case DXTTexture d:
                                        if (ImGui.TreeNodeEx($"DXT:{d.name}##{d.GetHashCode()}"))
                                        {
                                            ImGui.LabelText("Format", Encoding.ASCII.GetString(BitConverter.GetBytes((int)d.description.textureFormat)));
                                            ImGui.LabelText("Width", d.description.width.ToString());
                                            ImGui.LabelText("Height", d.description.height.ToString());
                                            ImGui.LabelText("Mipmaps", d.description.mipmaps.ToString());
                                            ImGui.TreePop();
                                        }
                                        break;
                                    case LodReferenceTexture l:
                                        if (ImGui.TreeNodeEx($"Ref:{l.name}##{l.GetHashCode()}"))
                                        {
                                            if (l.samplerState is not null)
                                            {
                                                DrawSamplerState(l.samplerState.Value);
                                            }
                                            ImGui.LabelText("Index", l.index.ToString());
                                        }
                                        break;
                                    default:
                                        if (ImGui.TreeNodeEx($"Empty", ImGuiTreeNodeFlags.Leaf))
                                        {
                                            ImGui.TreePop();
                                        }
                                        break;
                                }
                            });
                            break;
                    }
                    ImGui.EndChild();
                }
            }
            ImGui.EndTable();
        }
    }

    private unsafe void DrawMaterialList(MaterialList list)
    {
        DrawList("Types", list.materialTypes, (i, t) =>
        {
            DrawList($"[{i}] {t.name}:{t.effect}", t.elements, (i, e) =>
            {
                if (ImGui.TreeNode($"{e.name} ({e.type}:{e.count})"))
                {
                    ImGui.TreePop();
                }
            });
        });
        DrawMaterials(list.materialTypes, list.materials);
    }

    private unsafe void DrawMaterials(IReadOnlyList<MaterialType> materialTypes, IReadOnlyList<Material> materials)
    {
        DrawList("Materials", materials, (i, m) =>
        {
            var type = materialTypes[m.typeIndex];
            DrawList($"[{i}] {m.typeIndex}:{type.name}", m.elements, (i, e) =>
            {
                if (ImGui.TreeNode($"[{i}] {type.elements[i].name}({type.elements[i].count})"))
                {
                    int j = 0;
                    foreach (var item in e)
                    {
                        if (ImGui.TreeNodeEx($"[{j}]: {item.value}", ImGuiTreeNodeFlags.Leaf))
                        {
                            ImGui.TreePop();
                        }
                        j++;
                    }
                    ImGui.TreePop();
                }
            });
        });
    }

    private unsafe ImGuiTreeNodeFlags DrawSelectableParentList<T,U>(T parent, Func<T, string> name, IReadOnlyList<U> items, Action<int, U> draw) where T : class where U : class
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if (Model.Models.Length == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (selectedItem == parent) flags |= ImGuiTreeNodeFlags.Selected;
        bool nodeOpen = ImGui.TreeNodeEx(name(parent), flags);
        if(ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            selectedItem = parent;
        }
        if (nodeOpen)
        {
            int i = 0;
            foreach (var item in items)
            {
                ImGui.PushID(i);
                draw(i, item);
                ImGui.PopID();
                i++;
            }
            ImGui.TreePop();
        }

        return flags;
    }

    private unsafe ImGuiTreeNodeFlags DrawSelectableList<T>(string id, IReadOnlyList<T> items, Func<int, T, string> name) where T : class
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;
        if (items.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (ImGui.TreeNodeEx(id, flags))
        {
            int i = 0;
            foreach (var item in items)
            {
                ImGui.PushID(i);
                DrawSelectableLeaf(item,v => name(i, v));
                ImGui.PopID();
                i++;
            }
            ImGui.TreePop();
        }

        return flags;
    }

    private unsafe void DrawSelectableLeaf<T>(T item, Func<T, string> name) where T : class
    {
        var flags = ImGuiTreeNodeFlags.Leaf;
        if (selectedItem == item) flags |= ImGuiTreeNodeFlags.Selected;
        bool nodeOpen = ImGui.TreeNodeEx(name(item), flags);
        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
        {
            selectedItem = item;
        }
        if (nodeOpen) ImGui.TreePop();
    }

    private unsafe void DrawSamplerState(SamplerState samplerState)
    {
        ImGui.LabelText("Mag", samplerState.magFilter.ToString());
        ImGui.LabelText("Min", samplerState.minFilter.ToString());
        ImGui.LabelText("Mip", samplerState.mipFilter.ToString());

        ImGui.LabelText("WrapS", samplerState.u.ToString());
        ImGui.LabelText("WrapT", samplerState.v.ToString());
        ImGui.LabelText("WrapR", samplerState.w.ToString());
    }

    public void DrawPrimitive(string id, Primitive primitive)
    {
        if(ImGui.TreeNodeEx(id))
        {
            ImGui.LabelText("Effect", primitive.effect.ToString());
            ImGui.LabelText("Vertex Declaration", primitive.vertexDeclaration.ToString());
            ImGui.LabelText("Primitive Type", primitive.primitiveType.ToString());
            ImGui.LabelText("Index Buffer", primitive.indexBuffer.ToString());
            ImGui.LabelText("Element Index", primitive.elementIndex.ToString());
            ImGui.LabelText("Element List", primitive.elementListIndex.ToString());
            ImGui.LabelText("Base Material", primitive.baseMaterial.ToString());
            ImGui.LabelText("Instance Material", primitive.instanceMaterial.ToString());
            if(ImGui.TreeNodeEx("Blend State"))
            {
                DrawBlendState(primitive.blendState);
                ImGui.TreePop();
            }

            DrawList("Lods", primitive.lods, (i, l) =>
            {
                if (ImGui.TreeNodeEx($"Lod [{i}]: Index {l.indexOffset}-{l.indexOffset + l.indexCount}({l.indexCount}), Vertex {l.vertexOffset}-{l.vertexOffset+l.vertexCount}({l.vertexCount})"))
                {
                    ImGui.LabelText("Index Offset", l.indexOffset.ToString());
                    ImGui.LabelText("Index Count", l.indexCount.ToString());
                    ImGui.LabelText("Vertex Offset", l.vertexOffset.ToString());
                    ImGui.LabelText("Vertex Count", l.vertexCount.ToString());
                    ImGui.TreePop();
                }
            });
            DrawList("Primitve Streams", primitive.primitiveStreams, (i, p) =>
            {
                if (ImGui.TreeNodeEx($"Stream [{i}]"))
                {
                    ImGui.LabelText("Vertex Buffer", p.vertexBuffer.ToString());
                    ImGui.LabelText("Stream Index", p.streamIndex.ToString());
                    ImGui.LabelText("Vertex Offset", p.vertexOffset.ToString());
                    ImGui.LabelText("Frequency", p.frequency.ToString());

                    ImGui.LabelText("Material", p.material.ToString());
                    ImGui.TreePop();
                }
            });
            ImGui.TreePop();
        }
    }
    public void InputMatrix4x4(ref Matrix4x4 transformation)
    {
        if(Matrix4x4.Decompose(transformation, out var scale, out var rotation, out var translation))
        {
            var eulerRotation = ToEulerAngles(rotation);
            bool changed = false;
            changed |= ImGui.InputFloat3("Translation", ref translation);
            changed |= ImGui.InputFloat3("Rotation", ref eulerRotation);
            changed |= ImGui.InputFloat3("Scale", ref scale);
            if (changed)
            {
                transformation = Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(eulerRotation.Y, eulerRotation.X, eulerRotation.Z)) * Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(translation);
            }
        }
    }
    public void DrawList<T>(string name, IReadOnlyList<T> elements, Action<int, T> draw) => DrawList(name, elements, ImGuiTreeNodeFlags.None, draw);
    public void DrawList<T>(string name, IReadOnlyList<T> elements, ImGuiTreeNodeFlags flags, Action<int, T> draw)
    {
        if (elements.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
        if (ImGui.TreeNodeEx(name, flags))
        {
            for (int i = 0; i < elements.Count; i++)
            {
                T? item = elements[i];
                ImGui.PushID(i);
                draw(i, item);
                ImGui.PopID();
            }
            ImGui.TreePop();
        }
    }
    public void DrawBlendState(BlendState blendState)
    {
        ImGui.LabelText("Source Blend", blendState.sourceBlend.ToString());
        ImGui.LabelText("Destination Blend", blendState.destinationBlend.ToString());
        ImGui.LabelText("Blend Operation", blendState.blendOperation.ToString());
        ImGui.LabelText("Alpha Blend Enable", blendState.alphaBlendEnable.ToString());
        ImGui.LabelText("Alpha Test Enable", blendState.alphaTestEnable.ToString());
        ImGui.LabelText("Alpha Ref", blendState.alphaRef.ToString());
        ImGui.LabelText("Alpha To Mask Enable", blendState.alphaToMaskEnable.ToString());
        ImGui.LabelText("Cull Mode", blendState.cullMode.ToString());
        ImGui.LabelText("Z Bias", blendState.zBias.ToString());
    }
    
    public static Quaternion ToQuaternion(Vector3 v)
    {

        float cy = (float)Math.Cos(v.Z * 0.5);
        float sy = (float)Math.Sin(v.Z * 0.5);
        float cp = (float)Math.Cos(v.Y * 0.5);
        float sp = (float)Math.Sin(v.Y * 0.5);
        float cr = (float)Math.Cos(v.X * 0.5);
        float sr = (float)Math.Sin(v.X * 0.5);

        return new Quaternion
        {
            W = (cr * cp * cy + sr * sp * sy),
            X = (sr * cp * cy - cr * sp * sy),
            Y = (cr * sp * cy + sr * cp * sy),
            Z = (cr * cp * sy - sr * sp * cy)
        };

    }

    public static Vector3 ToEulerAngles(Quaternion q)
    {
        Vector3 angles = new();

        // roll / x
        double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
        double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch / y
        double sinp = 2 * (q.W * q.Y - q.Z * q.X);
        if (Math.Abs(sinp) >= 1)
        {
            angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
        }
        else
        {
            angles.Y = (float)Math.Asin(sinp);
        }

        // yaw / z
        double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }

    class ModelTexture
    {
        public DXTTexture textureSpecification;
        public uint texHandle;
        public ModelTexture(DXTTexture textureSpecification, uint texHandle)
        {
            this.textureSpecification = textureSpecification;
            this.texHandle = texHandle;
        }
        public static int CalculateUncompressedSize(int width, int height, int bitsPerPixel)
        {
            return (int)(width * height * bitsPerPixel/8f);
        }
        public static int CalculateCompressedSize(int width, int height, int blockSize)
        {
            return ((width + 3) / 4) * ((height + 3) / 4) * blockSize;
        }
        public static Func<int, int, int> Resolve(Func<int, int, int, int> f, int size) => (w, h) => f(w, h, size);
        public static ModelTexture Create(GL gl, DXTTexture textureSpecification)
        {
            (GLInternalFormat format, Func<int, int, int>? sizeCalculator) = textureSpecification.description.textureFormat switch
            {
                DXTFormat.Uncompressed3 => (GLInternalFormat.Rgba16, Resolve(CalculateUncompressedSize, 16)),
                DXTFormat.DXT1 => (GLInternalFormat.CompressedRgbaS3TcDxt1Ext, Resolve(CalculateCompressedSize, 8)),
                DXTFormat.DXT3 => (GLInternalFormat.CompressedRgbaS3TcDxt3Ext, Resolve(CalculateCompressedSize, 16)),
                DXTFormat.DXT5 => (GLInternalFormat.CompressedRgbaS3TcDxt5Ext, Resolve(CalculateCompressedSize, 16)),
                _ => (GLInternalFormat.Rgba, null)
            };
            if(sizeCalculator is null)
            {
                return new ModelTexture(textureSpecification, uint.MaxValue);
            }
            var texture = gl.GenTexture();
            gl.BindTexture(GLTextureTarget.Texture2D, texture);

            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.BaseLevel, 0);
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MaxLevel, (int)textureSpecification.description.mipmaps - 1);
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)(textureSpecification.samplerState.magFilter switch
            {
                TextureFilter.Point => GLTextureMagFilter.Nearest,
                TextureFilter.Linear => GLTextureMagFilter.Linear,
                _ => GLTextureMagFilter.Linear,
            }));
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)((textureSpecification.samplerState.minFilter, textureSpecification.samplerState.mipFilter) switch
            {
                (TextureFilter.Point, TextureFilter.Point) => GLTextureMinFilter.NearestMipmapNearest,
                (TextureFilter.Linear, TextureFilter.Point) => GLTextureMinFilter.LinearMipmapNearest,
                (TextureFilter.Point, TextureFilter.Linear) => GLTextureMinFilter.NearestMipmapLinear,
                (TextureFilter.Linear, TextureFilter.Linear) => GLTextureMinFilter.LinearMipmapLinear,
                _ => GLTextureMinFilter.Linear,
            }));
            //gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.mip, GL_LINEAR_MIPMAP_LINEAR); // don't forget to enable mipmaping
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapS, (int)(textureSpecification.samplerState.u switch
            {
                TextureWrap.Wrap => GLTextureWrapMode.Repeat,
                TextureWrap.Mirror => GLTextureWrapMode.MirroredRepeat,
                TextureWrap.Clamp => GLTextureWrapMode.ClampToEdge,
                TextureWrap.Border => GLTextureWrapMode.ClampToBorder,
                _ => GLTextureWrapMode.Repeat,
            }));
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapT, (int)(textureSpecification.samplerState.v switch
            {
                TextureWrap.Wrap => GLTextureWrapMode.Repeat,
                TextureWrap.Mirror => GLTextureWrapMode.MirroredRepeat,
                TextureWrap.Clamp => GLTextureWrapMode.ClampToEdge,
                TextureWrap.Border => GLTextureWrapMode.ClampToBorder,
                _ => GLTextureWrapMode.Repeat,
            }));
            gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapR, (int)(textureSpecification.samplerState.w switch
            {
                TextureWrap.Wrap => GLTextureWrapMode.Repeat,
                TextureWrap.Mirror => GLTextureWrapMode.MirroredRepeat,
                TextureWrap.Clamp => GLTextureWrapMode.ClampToEdge,
                TextureWrap.Border => GLTextureWrapMode.ClampToBorder,
                _ => GLTextureWrapMode.Repeat,
            }));
            int offset = 0;
            int size;
            int w = textureSpecification.description.width;
            int h = textureSpecification.description.height;
            int mipMaps = textureSpecification.description.mipmaps;
            for (int i = 0; i < mipMaps; i++)
            {
                if(offset == textureSpecification.bytes.Length)
                {
                    gl.GenerateMipmap(GLTextureTarget.Texture2D);
                    break;
                }
                if (w < 1)
                {
                    w = 1;
                }
                if(h == 0)
                {
                    h = 1;
                }
                size = sizeCalculator(w, h);
                if(format == GLInternalFormat.Rgba16)
                {
                    gl.TexImage2D(GLTextureTarget.Texture2D, (int)i, format, (int)w, (int)h, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedShort4444, textureSpecification.bytes.AsSpan()[(int)offset..]);
                }
                else
                {
                    gl.CompressedTexImage2D(GLTextureTarget.Texture2D, (int)i, format, (int)w, (int)h, 0, (int)size, textureSpecification.bytes.AsSpan()[(int)offset..]);
                }
                offset += size;
                w /= 2;
                h /= 2;
            }
            gl.BindTexture(GLTextureTarget.Texture2D, 0);
            return new ModelTexture(textureSpecification, texture);
        }
    }
    
    private static void GlCheckError(GL gl, [CallerFilePath] string? filePath = null, [CallerLineNumber] int? line = null)
    {
        var error = (GLErrorCode)gl.GetError();
        if (error != GLErrorCode.NoError)
        {
            Console.WriteLine($"OpenGL Error: {error} {filePath}:{line}");
        }
    }
}