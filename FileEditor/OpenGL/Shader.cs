using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.OpenGL;
using static OpenGL;
namespace Editor.OpenGL;
public class Shader : GLObject
{
    Shader(uint handle) : base(handle)
    {
    }

    public void Use()
    {
        GL3.UseProgram(Handle);
    }
    public void SetBool(string name, bool value) 
    {
        GL3.Uniform1i(GL3.GetUniformLocation(Handle, name), value ? 1 : 0);
    }
    public void SetInt(string name, in int value) 
    {
        GL3.Uniform1i(GL3.GetUniformLocation(Handle, name), value); 
    }
    public void SetFloat(string name, in float value)
    {
        GL3.Uniform1f(GL3.GetUniformLocation(Handle, name), value); 
    }
    public void SetMatrix(string name, Matrix4x4 value)
    {
        GL3.UniformMatrix4fv(GL3.GetUniformLocation(Handle, name), 1, false, ref value.M11);
    }

    public static Shader Create(string path)
    {
        var vertex = GL3.CreateShader(GLShaderType.VertexShader);
        var vertSource = File.ReadAllText(Path.ChangeExtension(path, ".vert"));
        GL3.ShaderSource(vertex, vertSource);
        GL3.CompileShader(vertex);
        int success;
        GL3.GetShaderiv(vertex, GLShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL3.GetShaderInfoLog(vertex);
            throw new Exception(infoLog);
        };

        var fragment = GL3.CreateShader(GLShaderType.FragmentShader);
        var fragSource = File.ReadAllText(Path.ChangeExtension(path, ".frag"));
        GL3.ShaderSource(fragment, fragSource);
        GL3.CompileShader(fragment);
        GL3.GetShaderiv(fragment, GLShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL3.GetShaderInfoLog(fragment);
            throw new Exception(infoLog);
        };

        var id = GL3.CreateProgram();
        GL3.AttachShader(id, vertex);
        GL3.AttachShader(id, fragment);
        GL3.LinkProgram(id);

        GL3.GetProgramiv(id, GLProgramPropertyARB.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL3.GetProgramInfoLog(vertex);
            throw new Exception(infoLog);
        };

        GL3.DeleteShader(vertex);
        GL3.DeleteShader(fragment);
        return new Shader(id);
    }
}
