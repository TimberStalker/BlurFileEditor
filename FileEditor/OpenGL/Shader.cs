using Editor.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Editor.Rendering.GL;

namespace Editor.OpenGL;
public class Shader
{
    uint handle;

    Shader(uint handle)
    {
        this.handle = handle;
    }

    public void Use()
    {
        glUseProgram(handle);
    }
    public void SetBool(string name, bool value) 
    {         
        glUniform1i(glGetUniformLocation(handle, name), value ? 1 : 0);
    }
    public void SetInt(string name, int value) 
    { 
        glUniform1i(glGetUniformLocation(handle, name), value); 
    }
    public void SetFloat(string name, float value)
    { 
        glUniform1f(glGetUniformLocation(handle, name), value); 
    }
    public void SetMatrix(string name, Matrix4x4 value)
    {
        glUniformMatrix4fv(glGetUniformLocation(handle, name), 1, true, value.M11);
    }

    public static implicit operator uint(Shader texture)
    {
        return texture.handle;
    }
    public static implicit operator nint(Shader texture)
    {
        return (nint)texture.handle;
    }

    public static Shader Create(string path)
    {
        var vertex = glCreateShader(GL_VERTEX_SHADER);
        glShaderSource(vertex, 1, File.ReadAllLines(Path.ChangeExtension(path, ".vert")), 0);
        glCompileShader(vertex);
        int success;
        glGetShaderiv(vertex, GL_COMPILE_STATUS, out success);
        if (success == 0)
        {
            glGetShaderInfoLog(vertex, 512, out _, out string infoLog);
            throw new Exception(infoLog);
        };

        var fragment = glCreateShader(GL_FRAGMENT_SHADER);
        glShaderSource(fragment, 1, File.ReadAllLines(Path.ChangeExtension(path, ".frag")), 0);
        glCompileShader(fragment);
        glGetShaderiv(fragment, GL_COMPILE_STATUS, out success);
        if (success == 0)
        {
            glGetShaderInfoLog(fragment, 512, out _, out string infoLog);
            throw new Exception(infoLog);
        };

        var id = glCreateProgram();
        glAttachShader(id, vertex);
        glAttachShader(id, fragment);
        glLinkProgram(id);

        glGetProgramiv(id, GL_COMPILE_STATUS, out success);
        if (success == 0)
        {
            glGetProgramInfoLog(vertex, 512, out _, out string infoLog);
            throw new Exception(infoLog);
        };

        glDeleteShader(vertex);
        glDeleteShader(fragment);
        return new Shader(id);
    }
}
