using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Editor.Editing.Models;

public class EditScene
{
    public string Name { get; set; } = "";
    public List<EditModel> Models { get; } = [];
}

public class EditModel
{
    public string Name { get; set; } = "";
    public List<EditModel> Models { get; } = [];
}
public class EditMesh
{
    readonly AttributeLayer<float> verts = new(3);
    readonly AttributeLayer<int> faces = new(3);
    readonly AttributeLayer<int> edges = new(2);
    readonly List<IAttrbuteLayer> layers = [];

    public void RemoveVerticies(Range range)
    {

    }
    public void SwapVerticies(Range x, Range y)
    {

    }
}
public interface IAttrbuteLayer
{

}
public class AttributeLayer<T> : IAttrbuteLayer
{
    public readonly int attributeSize;
    List<T> data = [];

    public AttributeLayer(int attributeSize)
    {
        this.attributeSize = attributeSize;
    }
}
public enum AttributeCompression
{
    Uncompressed,
    Half,
}
public class EditMaterial
{
    public string Name { get; set; } = "";
}
