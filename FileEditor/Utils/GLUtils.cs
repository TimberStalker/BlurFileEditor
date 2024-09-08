using Editor.Rendering;
using static Editor.Rendering.GL;

namespace Editor.Utils {

    public static class GLUtils {
        
        /// <summary>
        /// Check the <see cref="GL.GL_COMPILE_STATUS"/> of a previously compiled shader.
        /// </summary>
        /// <remarks>
        /// Will throw an <see cref="Exception"/> if the check result is <see cref="GL.GL_FALSE"/>.
        /// </remarks>
        /// <param name="_handle">The handle of the compiler Shader to check.</param>
        public static void CheckShader(uint _handle) {
            glGetShaderiv(_handle, GL_COMPILE_STATUS, out int _status);
            if (_status != GL_FALSE) return; //We're good to go
            
            glGetShaderiv(_handle, GL_INFO_LOG_LENGTH, out int _logLength);
            glGetShaderInfoLog(_handle, _logLength, out _, out string _log);
            throw new Exception($"Check Shader failed => {_log}");
        }

        /// <summary>
        /// Check the <see cref="GL.GL_LINK_STATUS"/> of a previous created and linked ProgramShader.
        /// </summary>
        /// <remarks>
        /// Will throw an <see cref="Exception"/> if the check result is <see cref="GL.GL_FALSE"/>.
        /// </remarks>
        /// <param name="_handle">The handle of the ShaderProgram to check.</param>
        public static void CheckProgram(uint _handle) {
            glGetProgramiv(_handle, GL_LINK_STATUS, out int _status);
            if (_status != GL_FALSE) return; //We're good to go
            
            glGetProgramiv(_handle, GL_INFO_LOG_LENGTH, out int _logLength);
            glGetProgramInfoLog(_handle, _logLength, out _, out string _log);
            throw new Exception($"Check ShaderProgram failed => {_log}");
        }
        
    }
    
}