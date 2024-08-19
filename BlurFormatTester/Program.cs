using BlurFormats.BlurData;
using BlurFormats.Loc;
using BlurFormats.Serialization;
using BlurFormats.Serialization.Entities;
using BlurFormats.Serialization.Types;
using BlurFormats.Utils;
using System.Diagnostics;
using System.Text;
using System.Xml;

string file = "amax";



////string file = "racingphysics";
//string file = "amax";
////string file = "multiplayerps3";

//using var writer = XmlWriter.Create(@$"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\{file}.xml", new XmlWriterSettings
//{
//    Indent = true
//});
////FlaskSerializer.XmlWriter = writer;
//var read = FlaskSerializer.Deserialize(@$"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\{file}.bin");
//writer.WriteStartDocument();
//writer.WriteStartElement("Serialization");
//
//writer.WriteStartElement("Types");
//
//foreach (var item in read.Item1)
//{
//    item.WriteXml(writer);
//}
//writer.WriteEndElement();
//writer.WriteStartElement("Records");
//foreach (var item in read.Item2)
//{
//    item.WriteXml(writer);
//}
//writer.WriteEndElement();
//
//writer.WriteEndElement();

var read = FlaskSerializer.Deserialize(@$"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\{file}.bin");

FlaskSerializer.Serialize(read.Item1, read.Item2, @$"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\{file}_serialized.bin");
//{
//    using var writer = XmlWriter.Create(@$"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\{file}_types.xml", new XmlWriterSettings
//    {
//        Indent = true
//    });
//    writer.WriteStartDocument();
//    writer.WriteStartElement("types");
//    List<string> typeNames = new List<string> ();
//
//    foreach (var record in read.Item2)
//    {
//        typeNames.TryAdd(record.Entity.Type.Name);
//    }
//    HashSet<IEntity> searchedEntities = new HashSet<IEntity>();
//    int index = 0;
//    foreach (var record in read.Item2)
//    {
//        List<string> usedNames = new List<string>();
//        SearchEntity(record.Entity, usedNames, searchedEntities, writer);
//        foreach (var item in usedNames)
//        {
//            typeNames.TryAdd(item);
//        }
//        index++;
//    }
//    typeNames.Remove("sz8");
//    for (int i = 0; i < read.Item1.Count; i++)
//    {
//        writer.WriteStartElement(read.Item1[i].Name);
//        if(i < typeNames.Count)
//        {
//            writer.WriteAttributeString("t", typeNames[i]);
//            writer.WriteAttributeString("is", (read.Item1[i].Name == typeNames[i]).ToString());
//        }
//        writer.WriteEndElement();
//    }
//
//    writer.WriteEndElement();
//}
//
//void SearchEntity(IEntity entity, List<string> typeNames, HashSet<IEntity> searchedEntities, XmlWriter writer)
//{
//    if (!searchedEntities.Add(entity)) return;
//    if(entity is StructureEntity s)
//    {
//        List<ArrayEntity> arrs = new List<ArrayEntity>();
//        List<IEntity> nonRefEntities = new List<IEntity>();
//        foreach (var item in s.Values)
//        {
//            if (item is ReferenceEntity r)
//            {
//                if(r.Reference is ArrayEntity arr && arr.Entities.Count != 0)
//                {
//                    typeNames.TryAdd(arr.Type.Name);
//                    arrs.Add(arr);
//                }
//                else
//                {
//                    SearchEntity(item, typeNames, searchedEntities, writer);
//                }
//            }
//            else
//            {
//                nonRefEntities.Add(item);
//            }
//        }
//        foreach (var arr in arrs)
//        {
//            SearchEntity(arr, typeNames, searchedEntities, writer);
//        }
//        foreach (var item in nonRefEntities)
//        {
//            SearchEntity(item, typeNames, searchedEntities, writer);
//        }
//    } else if(entity is ArrayEntity ae)
//    {
//        foreach (var item in ae.Entities)
//        {
//            SearchEntity(ae, typeNames, searchedEntities, writer);
//            typeNames.TryAdd(item.Reference.Type.Name);
//        }
//    } else if(entity is ReferenceEntity r)
//    {
//        if (r.Reference is NullEntity) return;
//        if (r.Reference is ArrayEntity a && a.Entities.Count == 0) return;
//        if (r.Reference is RecordReferenceEntity) return;
//
//        typeNames.TryAdd(r.Type.Name);
//        typeNames.TryAdd(r.Reference.Type.Name);
//        SearchEntity(r.Reference, typeNames, searchedEntities, writer);
//    }
//}
////FlaskSerializer.Serialize(read.Item1, read.Item2, @$"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\{file}_serialized.bin");
static class Extensions
{
    public static bool TryAdd<T>(this List<T> list, T item)
    {
        if(list.Contains(item)) return false;
        list.Add(item);
        return true;
    }
}