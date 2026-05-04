using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using BlurFileFormats.Models;
using BlurFileFormats.Shaders;
using Hexa.NET.OpenGL;

namespace Editor.Panels;
public class RenderMaterial
{
    public readonly FXB shader;

    public RenderMaterial(FXB shader)
    {
        this.shader = shader;
    }
}
public class ModelRenderInfo
{
    public const string collisionShaderVertex = """
        #version 330 core
        uniform mat4 projection;
        uniform mat4 view;
        uniform mat4 model;

        layout (location = 0) in vec3 position;
        
        void main() {
            gl_Position = projection * view * model * vec4(position, 1.0);
        }
        """;
    public const string collisionShaderFragment = """
        #version 330 core

        uniform vec3 color;
        out vec4 fragColor;

        void main() {
            fragColor = color;
        }
        """;
    public readonly List<RenderModel> models;
    public readonly List<RenderElement> elements;
    public readonly List<RenderMaterial> materials;

    public ModelRenderInfo(List<RenderModel> models, List<RenderElement> elements, List<RenderMaterial> renderMaterials)
    {
        this.models = models;
        this.elements = elements;
        this.materials = renderMaterials;
    }
    public static uint CreateShaderFor(GL gl, VertexDeclaration declaration)
    {
        StringBuilder vertexTopBuilder = new StringBuilder(100);
        StringBuilder vertexMainBuilder = new StringBuilder(100);
        StringBuilder fragTopBuilder = new StringBuilder(100);
        StringBuilder fragMainBuilder = new StringBuilder(100);
        vertexTopBuilder.AppendLine("#version 330 core");
        vertexTopBuilder.AppendLine("uniform mat4 projection;");
        vertexTopBuilder.AppendLine("uniform mat4 view;");
        vertexTopBuilder.AppendLine("uniform mat4 model;");

        fragTopBuilder.AppendLine("#version 330 core");
        fragTopBuilder.AppendLine("uniform int selection;");
        fragTopBuilder.AppendLine("uniform float lightStrength;");
        fragTopBuilder.AppendLine("uniform vec3 lightDirection;");
        fragTopBuilder.AppendLine("uniform vec3 ambientColor;");
        fragTopBuilder.AppendLine("uniform vec3 diffuseColor;");
        fragTopBuilder.AppendLine("out vec4 fragColor;");

        vertexMainBuilder.AppendLine("void main() {");
        fragMainBuilder.AppendLine("void main() {");
        fragMainBuilder.AppendLine("vec4 color = vec4(0.0);");
        int normalCount = 0;
        for (int i = 0; i < declaration.elements.Length; i++)
        {
            VertexElement item = declaration.elements[i];
            string type = item.type switch
            {
                VertexElementType.Vec2S16 => "vec2",
                VertexElementType.Vec4S16 => "vec4",
                VertexElementType.Vec2NS16 => "vec2",
                VertexElementType.Vec4NS16 => "vec4",
                VertexElementType.F32 => "float",
                VertexElementType.Vec2F32 => "vec2",
                VertexElementType.Vec3F32 => "vec3",
                VertexElementType.Vec4F32 => "vec4",
                VertexElementType.Vec2F16 => "vec2",
                VertexElementType.Vec4F16 => "vec4",
                VertexElementType.Vec4U8 => "vec4",
                VertexElementType.Vec4NU8 => "vec4",
                VertexElementType.Vec3NSHHD => "vec3",
                VertexElementType.Vec3NSDDD => "vec3",
                _ => throw new NotSupportedException()
            };
            string name = item.usage.ToString().ToLower() + item.usageIndex.ToString();
            vertexTopBuilder.AppendLine($"layout (location = {i}) in {type} {name};");
            vertexTopBuilder.AppendLine($"out {type} {name}Frag;");

            fragTopBuilder.AppendLine($"in {type} {name}Frag;");

            vertexMainBuilder.AppendLine($"{name}Frag = {name};");
            if(item.usage == VertexElementUsage.Position)
            {
                switch(type)
                {
                    case "float":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}, 0.0, 0.0, 1.0);");
                        break;
                    case "vec2":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}, 0.0, 1.0);");
                        break;
                    case "vec3":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}, 1.0);");
                        break;
                    case "vec4":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}.xyz, 1.0);");
                        break;
                }
            } else if(item.usage == VertexElementUsage.Normal)
            {
                switch (type)
                {
                    case "vec3":
                        normalCount++;
                        fragMainBuilder.AppendLine($"float light{item.usageIndex} = dot(lightDirection, {name}Frag);");
                        break;
                    case "vec4":
                        normalCount++;
                        fragMainBuilder.AppendLine($"float light{item.usageIndex} = dot(lightDirection, {name}Frag.xyz);");
                        break;
                }
            }
            string selection = $"clamp(1 - abs({i} - selection), 0, 1)";
            switch (type)
            {
                case "float":
                    fragMainBuilder.AppendLine($"color += vec4({name}Frag, 0.0, 0.0, 1.0) * ({selection});");
                    break;
                case "vec2":
                    fragMainBuilder.AppendLine($"color += vec4({name}Frag, 0.0, 1.0) * ({selection});");
                    break;
                case "vec3":
                    fragMainBuilder.AppendLine($"color += vec4({name}Frag, 1.0) * ({selection});");
                    break;
                case "vec4":
                    fragMainBuilder.AppendLine($"color += vec4({name}Frag.xyz, 1.0) * ({selection});");
                    break;
            }
        }
        vertexMainBuilder.AppendLine("}");

        fragMainBuilder.AppendLine($"color += vec4(diffuseColor, 1.0) * clamp(1 - abs(-1 - selection), 0, 1);");
        if(normalCount > 0)
        {
            fragMainBuilder.AppendLine("color = vec4(color.xyz * ((light0 + 1.0) / 2.0) * lightStrength + ambientColor, color.w);");

        }
        fragMainBuilder.AppendLine("fragColor = color;");
        fragMainBuilder.AppendLine("}");

        var vert = gl.CreateShader(GLShaderType.VertexShader);
        var frag = gl.CreateShader(GLShaderType.FragmentShader);
        gl.ShaderSource(vert, vertexTopBuilder.ToString() + vertexMainBuilder.ToString());
        gl.ShaderSource(frag, fragTopBuilder.ToString() + fragMainBuilder.ToString());

        gl.CompileShader(vert);
        var infoLog = gl.GetShaderInfoLog(vert);
        gl.CompileShader(frag);
        infoLog = gl.GetShaderInfoLog(frag);

        var program = gl.CreateProgram();

        gl.AttachShader(program, vert);
        gl.AttachShader(program, frag);
        gl.LinkProgram(program);
        infoLog = gl.GetProgramInfoLog(program);

        gl.DeleteShader(vert);
        gl.DeleteShader(frag);
        return program;
    }
    public static string GetInputType(InputDeclaration input)
    {
        foreach (var item in input.annotations)
        {
            if(item.name == "format")
            {
                if(item.value is StringAnnotation a)
                {
                    return a.value switch
                    {
                        "F16_2" => "vec2",
                        _ => throw new NotSupportedException($"Unknown Input Format {a.value}")
                    };
                }
            }
        }
        return "vec4";
    }

    public static uint CreateShaderFor(GL gl, FXB fxb)
    {
        StringBuilder vertexTopBuilder = new StringBuilder(100);
        StringBuilder vertexMainBuilder = new StringBuilder(100);
        StringBuilder fragTopBuilder = new StringBuilder(100);
        StringBuilder fragMainBuilder = new StringBuilder(100);
        vertexTopBuilder.AppendLine("#version 330 core");
        vertexTopBuilder.AppendLine("uniform mat4 projection;");
        vertexTopBuilder.AppendLine("uniform mat4 view;");
        vertexTopBuilder.AppendLine("uniform mat4 model;");

        fragTopBuilder.AppendLine("#version 330 core");
        fragTopBuilder.AppendLine("uniform int selection;");
        fragTopBuilder.AppendLine("uniform float lightStrength;");
        fragTopBuilder.AppendLine("uniform vec3 lightDirection;");
        fragTopBuilder.AppendLine("uniform vec3 ambientColor;");
        fragTopBuilder.AppendLine("uniform vec3 diffuseColor;");
        fragTopBuilder.AppendLine("out vec4 fragColor;");

        vertexMainBuilder.AppendLine("void main() {");
        fragMainBuilder.AppendLine("void main() {");
        fragMainBuilder.AppendLine("fragColor = vec4(0.0);");
        int normalCount = 0;
        for (int i = 0; i < fxb.inputDeclarations.Count; i++)
        {
            InputDeclaration item = fxb.inputDeclarations[i];
            string type = GetInputType(item);
            string name = item.usage.ToString().ToLower() + item.usageIndex.ToString();
            vertexTopBuilder.AppendLine($"layout (location = {i}) in {type} {name};");
            vertexTopBuilder.AppendLine($"out {type} {name}Frag;");

            fragTopBuilder.AppendLine($"in {type} {name}Frag;");

            vertexMainBuilder.AppendLine($"{name}Frag = {name};");
            if (item.usage == FXSemantic.Position)
            {
                switch (type)
                {
                    case "float":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}, 0.0, 0.0, 1.0);");
                        break;
                    case "vec2":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}, 0.0, 1.0);");
                        break;
                    case "vec3":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}, 1.0);");
                        break;
                    case "vec4":
                        vertexMainBuilder.AppendLine($"gl_Position = projection * view * model * vec4({name}.xyz, 1.0);");
                        break;
                }
            }
            else if (item.usage == FXSemantic.Normal)
            {
                switch (type)
                {
                    case "vec3":
                        normalCount++;
                        fragMainBuilder.AppendLine($"float light{item.usageIndex} = dot(lightDirection, {name}Frag);");
                        break;
                    case "vec4":
                        normalCount++;
                        fragMainBuilder.AppendLine($"float light{item.usageIndex} = dot(lightDirection, {name}Frag.xyz);");
                        break;
                }
            }
            string selection = $"clamp(1 - abs({i} - selection), 0, 1)";
            switch (type)
            {
                case "float":
                    fragMainBuilder.AppendLine($"fragColor += vec4({name}Frag, 0.0, 0.0, 1.0) * ({selection});");
                    break;
                case "vec2":
                    fragMainBuilder.AppendLine($"fragColor += vec4({name}Frag, 0.0, 1.0) * ({selection});");
                    break;
                case "vec3":
                    fragMainBuilder.AppendLine($"fragColor += vec4({name}Frag, 1.0) * ({selection});");
                    break;
                case "vec4":
                    fragMainBuilder.AppendLine($"fragColor += vec4({name}Frag.xyz, 1.0) * ({selection});");
                    break;
            }
        }
        vertexMainBuilder.AppendLine("}");

        fragMainBuilder.AppendLine($"fragColor += vec4(diffuseColor, 1.0) * clamp(1 - abs(-1 - selection), 0, 1);");
        if (normalCount > 0)
        {
            fragMainBuilder.AppendLine("fragColor = vec4(fragColor.xyz * ((light0 + 1.0) / 2.0) * lightStrength + ambientColor, fragColor.w);");

        }
        fragMainBuilder.AppendLine("}");

        var vert = gl.CreateShader(GLShaderType.VertexShader);
        var frag = gl.CreateShader(GLShaderType.FragmentShader);
        gl.ShaderSource(vert, vertexTopBuilder.ToString() + vertexMainBuilder.ToString());
        gl.ShaderSource(frag, fragTopBuilder.ToString() + fragMainBuilder.ToString());

        gl.CompileShader(vert);
        var infoLog = gl.GetShaderInfoLog(vert);
        gl.CompileShader(frag);
        infoLog = gl.GetShaderInfoLog(frag);

        var program = gl.CreateProgram();

        gl.AttachShader(program, vert);
        gl.AttachShader(program, frag);
        gl.LinkProgram(program);
        infoLog = gl.GetProgramInfoLog(program);

        gl.DeleteShader(vert);
        gl.DeleteShader(frag);
        return program;
    }
    unsafe public static ModelRenderInfo Create(CPModel model, Projects.Project activeProject, GL gl)
    {
        Dictionary<string, FXB> loadedEffects = new Dictionary<string, FXB>();
        List<RenderMaterial> renderMaterials = new List<RenderMaterial>(model.Scene.effects.Length);
        foreach (var effect in model.Scene.effects)
        {
            if(!loadedEffects.TryGetValue(effect, out var fxb))
            {
                var path = Path.Combine(activeProject.BaseDirectory, "graphics", "shaders", "speed", effect + "b");
                fxb = FXBSerializer.Import(path);
                loadedEffects[effect] = fxb;
            }
            renderMaterials.Add(new RenderMaterial(fxb));
        }


        List<RenderModel> renderModels = [];
        for (int i = 0; i < model.Models.Length; i++)
        {
            var renderModel = new RenderModel
            {
                name = model.Models[i].name,
                transform = model.Models[i].transform,
            };
            renderModels.Add(renderModel);
            if (model.Models[i].parentIndex != -1)
            {
                renderModels[model.Models[i].parentIndex].children.Add(renderModel);
                renderModel.parent = renderModels[model.Models[i].parentIndex];
            }
        }
        List<RenderElement> elements = new List<RenderElement>(model.Elements.Length);
        for(int i = 0; i < model.Elements.Length; i++)
        {
            var element = new RenderElement
            {
                name = model.Elements[i].name,
                transform = model.Elements[i].transform,
                parentModel = renderModels[model.Elements[i].modelIndex]
            };
            renderModels[model.Elements[i].modelIndex].elements.Add(element);
            
            elements.Add(element);
            if (model.Elements[i].parentElement != -1)
            {
                elements[model.Elements[i].parentElement].children.Add(element);
                element.parentElement = elements[model.Elements[i].parentElement];
            }
        }


        Dictionary<int, List<(uint vert, int size, uint shader)>> vertexBuffersCollection = [];
        Dictionary<int, List<(uint ind, IndexType type)>> indexBuffersCollection = [];
        for (int i = 0; i < model.Scene.resourceBlocks.Length; i++)
        {
            ResourceBlock? item = model.Scene.resourceBlocks[i];
            if (item.lods.Length > 0)
            {
                uint[] shaders = new uint[item.lods[0].vertexBuffers.Length];
                uint[] vertexArrays = new uint[item.lods[0].vertexBuffers.Length];
                uint[] vertexBuffers = new uint[item.lods[0].vertexBuffers.Length];
                gl.GenVertexArrays(vertexBuffers.Length, vertexArrays);
                gl.GenBuffers(vertexBuffers.Length, vertexBuffers);

                gl.BindBuffer(GLBufferTargetARB.ElementArrayBuffer, 0);

                List<(uint vert, int size, uint shader)> vertexBufferList = new(vertexArrays.Length);
                for (int j = 0; j < vertexBuffers.Length; j++)
                {
                    uint vertArray = vertexArrays[j];
                    uint buffer = vertexBuffers[j];
                    var bufferData = item.lods[0].vertexBuffers[j];

                    shaders[j] = CreateShaderFor(gl, bufferData.declaration);
                    gl.BindBuffer(GLBufferTargetARB.ArrayBuffer, buffer);
                    gl.BufferData(GLBufferTargetARB.ArrayBuffer, bufferData.data.Length, bufferData.data.AsSpan(), GLBufferUsageARB.StaticRead);

                    gl.BindVertexArray(vertArray);

                    var structureSize = bufferData.vertexSize;
                    for (uint k = 0; k < bufferData.declaration.elements.Length; k++)
                    {
                        var element = bufferData.declaration.elements[(int)k];

                        var (size, elementCount, normalized, pointerType) = element.type switch
                        {
                            VertexElementType.Vec2S16 => (4, 2, false, GLVertexAttribPointerType.Short),
                            VertexElementType.Vec4S16 => (8, 4, false, GLVertexAttribPointerType.Short),
                            VertexElementType.Vec2NS16 => (4, 2, true, GLVertexAttribPointerType.Short),
                            VertexElementType.Vec4NS16 => (8, 4, true, GLVertexAttribPointerType.Short),
                            VertexElementType.F32 => (4, 1, false, GLVertexAttribPointerType.Float),
                            VertexElementType.Vec2F32 => (8, 2, false, GLVertexAttribPointerType.Float),
                            VertexElementType.Vec3F32 => (12, 3, false, GLVertexAttribPointerType.Float),
                            VertexElementType.Vec4F32 => (16, 4, false, GLVertexAttribPointerType.Float),
                            VertexElementType.Vec2F16 => (4, 2, false, GLVertexAttribPointerType.HalfFloat),
                            VertexElementType.Vec4F16 => (8, 4, false, GLVertexAttribPointerType.HalfFloat),
                            VertexElementType.Vec4U8 => (4, 4, true, GLVertexAttribPointerType.UnsignedByte),
                            VertexElementType.Vec4NU8 => (4, 4, true, GLVertexAttribPointerType.UnsignedByte),
                            VertexElementType.Vec3NSHHD => throw new NotSupportedException(),
                            VertexElementType.Vec3NSDDD => throw new NotSupportedException(),
                            _ => throw new NotSupportedException()
                        };

                        gl.VertexAttribPointer(k, elementCount, pointerType, normalized, structureSize, bufferData.offsets[(int)k]);
                        gl.EnableVertexAttribArray(k);
                    }
                    vertexBufferList.Add((vertexArrays[j], structureSize, shaders[j]));
                }
                gl.BindBuffer(GLBufferTargetARB.ArrayBuffer, 0);
                vertexBuffersCollection[i] = vertexBufferList;

                uint[] indexBuffers = new uint[item.lods[0].indexBuffers.Length];
                gl.GenBuffers(indexBuffers.Length, indexBuffers);
                List<(uint, IndexType)> indexBufferList = new(indexBuffers.Length);
                for (int j = 0; j < indexBuffers.Length; j++)
                {
                    uint buffer = indexBuffers[j];
                    var bufferData = item.lods[0].indexBuffers[j];
                    gl.BindBuffer(GLBufferTargetARB.ElementArrayBuffer, buffer);
                    gl.BufferData(GLBufferTargetARB.ElementArrayBuffer, bufferData.data.Length, bufferData.data.AsSpan(), GLBufferUsageARB.StaticRead);
                    indexBufferList.Add((buffer, bufferData.indexType));
                }
                indexBuffersCollection[i] = indexBufferList;
                gl.BindBuffer(GLBufferTargetARB.ElementArrayBuffer, 0);
            }
        }
        List<RenderNode> renderNodes = [];
        var renderingNodes = model.Scene.resourceBlocks[0].lods[0].renderingNodes;
        for (int i = 0; i < renderingNodes.Length; i++)
        {
            if (renderingNodes[i] is CommonRenderListNode common)
            {
                RenderModel targetModel = renderModels[common.parentIndex.index];
                foreach (var primitve in common.primitives)
                {
                    var primitveStream = primitve.primitiveStreams.First(s => s.streamIndex == 0);
                    var vertexBuffer = common.vertexBufferSources[primitveStream.vertexBuffer];
                    var indexBuffer = common.indexBufferSources[primitve.indexBuffer];

                    var vb = vertexBuffersCollection[vertexBuffer.block][vertexBuffer.index];
                    var ib = indexBuffersCollection[indexBuffer.block][indexBuffer.index];
                    var subMesh = new SubMesh
                    {
                        blendState = primitve.blendState,
                        indexBuffer = ib.ind,
                        indexType = ib.type,
                        primitiveType = primitve.primitiveType,
                        vertexBuffer = vb.vert,
                        shader = vb.shader,
                        baseVertex = primitveStream.vertexOffset / vb.size
                    };
                    foreach (var lod in primitve.lods)
                    {
                        subMesh.lods.Add(new MeshLod
                        {
                            indexRange = lod.indexOffset..(lod.indexOffset + lod.indexCount),
                            vertexRange = lod.vertexOffset..(lod.vertexOffset+ lod.vertexCount)
                        });
                    }
                    targetModel?.elements[primitve.elementIndex].mesh.subMeshes.Add(subMesh);
                }
            }
        }



        return new ModelRenderInfo(renderModels, elements, renderMaterials);
    }

    class RenderNode
    {
        public RenderNode? parentNode;
        public List<RenderNode> childNodes = [];
    }
}
[StructLayout(LayoutKind.Sequential)]
struct MatrixBlock
{
    public Matrix4x4 projection;
    public Matrix4x4 view;
    public Matrix4x4 model;
}
public class RenderModel
{
    public string name = "";
    public bool draw = true;
    public Matrix4x4 transform = Matrix4x4.Identity;
    public List<RenderModel> children = [];
    public List<RenderElement> elements = [];
    public RenderModel? parent;
}
public class RenderElement
{
    public string name = "";
    public required RenderModel parentModel;
    public bool draw = true;
    public Matrix4x4 transform = Matrix4x4.Identity;
    public Mesh mesh = new();
    public List<RenderElement> children = [];
    public RenderElement? parentElement;
}
public class Mesh
{
    public List<SubMesh> subMeshes = [];
}
public class SubMesh
{
    public BlendState blendState = new();
    public PrimitiveType primitiveType;
    public IndexType indexType;
    public uint vertexBuffer;
    public uint indexBuffer;
    public uint shader;
    public int shaderSelection;
    public List<MeshLod> lods = [];
    public int baseVertex;
}
public class MeshAttribute
{
    public int vertexCount;
    GLVertexAttribPointerType type;
    public int size;
    public bool normalized;
    public byte[] data;
    public int Count => data.Length / (size * type switch
    {
        GLVertexAttribPointerType.Byte => 1,
        GLVertexAttribPointerType.UnsignedByte => 1,
        GLVertexAttribPointerType.Short => 2,
        GLVertexAttribPointerType.UnsignedShort => 2,
        GLVertexAttribPointerType.Int => 4,
        GLVertexAttribPointerType.UnsignedInt => 4,
        GLVertexAttribPointerType.Float => 4,
        GLVertexAttribPointerType.Double => 8,
        GLVertexAttribPointerType.HalfFloat => 2,
        _ => throw new NotSupportedException()
    });
}
public class MeshLod
{
    public Range vertexRange;
    public Range indexRange;
}