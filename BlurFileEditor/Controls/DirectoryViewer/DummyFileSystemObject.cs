using System.IO;

namespace BlurFileEditor.Controls.DirectoryViewer;

internal class DummyFileSystemObject : FileSystemObject
{
	public DummyFileSystemObject() : base(new FileInfo("None"))
	{

	}
}