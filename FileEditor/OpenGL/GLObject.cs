using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Editor.OpenGL
{
    public abstract class GLObject : IDisposable
    {
        public uint Handle { get; }
        protected GLObject(uint handle)
        {
            Handle = handle;
        }
        public static implicit operator uint(GLObject obj)
        {
            return obj.Handle;
        }
        public static implicit operator nint(GLObject obj)
        {
            return (nint)obj.Handle;
        }

        bool disposed;
        protected virtual void Dispose(bool disposing) { }
        public void Dispose()
        {
            Dispose(true);
            disposed = true;
            GC.SuppressFinalize(this);
        }
        ~GLObject()
        {
            //Program.ExecuteOnMainThread(Dispose, false);
            Dispose(false);
            //disposed = true;
        }
    }
}
