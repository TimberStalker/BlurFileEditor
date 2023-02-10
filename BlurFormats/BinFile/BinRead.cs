using BlurFormats.BinFile.Definitions;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile;
public sealed class BinRead
{
    public List<DataTypeDefinition> DataTypeDefinitions { get; private set; } = new();
    public List<int> UnknownDefinitions { get; private set; } = new();
    public List<DatafieldDefinition> DataFieldDefinitions { get; private set; } = new();
    public List<EntityDefinition> EntityDefinitions { get; private set; } = new();
    public List<EntityNameDefinition> EntityNameDefinitions { get; private set; } = new();
    public List<Block3Definition> Block3Definitions { get; private set; } = new();
    public string StringsData { get; set; } = "";
    public static BinRead FromBytes(ref Reader reader)
    {

        string format = reader.ReadString(4);
        if (format != "KSLF") throw new Exception("Provided bytes are not of the KSLF(bin) format.");

        int magicNumber = reader.ReadInt();
        if (magicNumber != 2) throw new Exception($"Magic number is not correct. Should be '2' but instead is '{magicNumber}'.");

        var bin = new BinRead();

        Header dataTypesHeader = reader.Read<Header>();
        Header unknownHeader = reader.Read<Header>();
        Header dataFieldsHeader = reader.Read<Header>();
        Header encryptedStringsHeader = reader.Read<Header>();
        Header entityDefinitionsHeader = reader.Read<Header>();
        Header entityNamesHeader = reader.Read<Header>();
        Header block3Header = reader.Read<Header>();
        Header dataBlockHeader = reader.Read<Header>();

        for (int i = 0; i < dataTypesHeader.Length; i++)
            bin.DataTypeDefinitions.Add(reader.Read<DataTypeDefinition>());

        for(int i = 0; i < unknownHeader.Length; i++)
            bin.UnknownDefinitions.Add(reader.ReadInt());

        for (int i = 0; i < dataFieldsHeader.Length; i++)
            bin.DataFieldDefinitions.Add(reader.Read<DatafieldDefinition>());
        
        bin.StringsData = reader.ReadStringDecrypted(encryptedStringsHeader.Length);

        for (int i = 0; i < entityDefinitionsHeader.Length; i++)
            bin.EntityDefinitions.Add(reader.Read<EntityDefinition>());

        for (int i = 0; i < entityNamesHeader.Length; i++)
            bin.EntityNameDefinitions.Add(reader.Read<EntityNameDefinition>());
        
        for(int i = 0; i < block3Header.Length; i++)
        {
            bin.Block3Definitions.Add(reader.Read<Block3Definition>());
        }
        if (block3Header.Length > 0)
        {
            ushort unknownZero = reader.ReadUShort();
        }
        reader.Seek(dataBlockHeader.Start);
        return bin;
    }
}
