using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Editors;
public class FileEditorAttribute : Attribute
{
    public string Extension { get; }
	public FileEditorAttribute(string extension)
	{
        Extension = extension;
    }
    public static (FileEditorAttribute, Type)[] GetFileEditors()
    {
        return Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetCustomAttributes<FileEditorAttribute>().Select(a => (a,t))).Where(t => t.a is not null).ToArray();
    }
}
