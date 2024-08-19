using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Flask;
public class FlaskFile
{
    public int Version { get; set; }
    public List<Type> Types { get; } = [];
    public List<int> Inherits { get; } = [];
    public List<Field> Fields { get; } = [];
    public string TypesStringBlock { get; } = "";
    public List<Record> Records { get; } = [];
    public List<Entity> Entities { get; } = [];
    public List<Block> Blocks { get; } = [];

    public class Type
    {
        public short StringOffset { get; private set; }
        public short FieldCount { get; private set; }
        public short HasBase { get; private set; }
        public short StructureType { get; private set; }
        public short PrimitiveType { get; private set; }
        public short Size { get; private set; }
    }
    public class Field
    {
        public short NameOffset { get; private set; }
        public short BaseType { get; private set; }
        public short Offset { get; private set; }
        public byte FieldType { get; private set; }
        public byte IsArray { get; private set; }
    }
    public class Record
    {
        public uint ID { get; private set; }
        public short BaseType { get; private set; }
        public short Entity { get; private set; }
    }
    public class Entity
    {
        public int Record { get; private set; }
        public int Length { get; private set; }
        public int Size { get; private set; }
        public int NamesLength { get; private set; }
    }
    public class Block
    {
        public short DataType { get; set; }
        public short Count { get; set; }
        public short PointerType { get; set; }
    }
}
