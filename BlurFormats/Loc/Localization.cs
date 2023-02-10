using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Loc;
public class Localization
{
    public ObservableCollection<Language> Languages { get; } = new ObservableCollection<Language>();
    public ObservableCollection<LocString> Strings { get; } = new ObservableCollection<LocString>();

    public static Localization CreateFrom(byte[] bytes)
    {
        var reader = new Reader(bytes);
        var loc = new Localization();

        string format = reader.ReadString(4);
        if (format != "OLTX") throw new Exception($"{format} is not a handled loc format.");
        int magicNumber = reader.ReadInt();
        if (magicNumber != 1) throw new Exception($"Magic number must be 1; intead is {magicNumber}.");

        int headerSize = reader.ReadInt();
        int langStarts = reader.ReadInt();
        
        int unknownC = reader.ReadInt();
        int languageCount = reader.ReadInt();
        int unknown84 = reader.ReadInt();

        var languageHeaders = new List<LanguageHeader>();
        for(int i = 0; i < languageCount; i++)
        {
            var langHeader = reader.Read<LanguageHeader>();
            languageHeaders.Add(langHeader);
            loc.Languages.Add(new Language() { Name = langHeader.Name.Trim() });
        }

        int stringCount = reader.ReadInt();
        int byteSize = reader.ReadInt();

        var stringHeaders = new (uint id, uint pos)[stringCount];

        for(int i = 0; i < stringCount; i++)
        {
            uint stringID = reader.ReadUInt();
            uint stringPos = reader.ReadUInt();
            stringHeaders[i] = (stringID, stringPos);
        }
        Dictionary<uint, LocString> locDictionary = new Dictionary<uint, LocString>();
        for (int i = 0; i < stringCount; i++)
        {
            var text = reader.ReadStringTerminated();
            var locString = new LocString()
            {
                Id = stringHeaders[i].id,
                Header = text
            };
            loc.Strings.Add(locString);
            locDictionary[locString.Id] = locString;
        }

        for (int i = 0; i < languageCount; i++)
        {
            var language = loc.Languages[i];

            int langStringCount = reader.ReadInt();
            int langByteSize = reader.ReadInt();
            var langStringHeaders = new (uint id, uint pos)[langStringCount];

            for (int j = 0; j < langStringCount; j++)
            {
                uint stringID = reader.ReadUInt();
                uint stringPos = reader.ReadUInt();
                langStringHeaders[j] = (stringID, stringPos);
            }

            for (int j = 0; j < langStringCount; j++)
            {
                var text = reader.ReadUnicodeStringTerminated();
                locDictionary[langStringHeaders[j].id].Texts[language] = text;
            }
        }
        return loc;
    }
    public static byte[] ToBytes(Localization localization)
    {
        int asciiOffset = localization.Strings.Count * 8;
        byte[] asciiBytes;
        using var asciiStream = new MemoryStream();
        using (var asciiWriter = new BinaryWriter(asciiStream))
        {
            asciiWriter.Write(localization.Strings.Count);
            asciiWriter.Write(8);
            using var asciiTextStream = new MemoryStream();
            using (var writer = new BinaryWriter(asciiTextStream))
            {
                foreach (var item in localization.Strings)
                {
                    asciiWriter.Write(item.Id);
                    asciiWriter.Write((int)asciiTextStream.Position + asciiOffset + 8);
                    writer.Write(Encoding.ASCII.GetBytes(item.Header));
                    writer.Write((byte)0);
                }
            }
            asciiWriter.Write(asciiTextStream.ToArray());
            asciiBytes = asciiStream.ToArray();
        }

        var unicodeStreams = GetUnicodeLanguageStreams(localization.Languages, localization.Strings);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write('O');
            writer.Write('L');
            writer.Write('T');
            writer.Write('X');
            writer.Write(1);
            writer.Write(16);

            using var headerStream = new MemoryStream();
            using (var headerWriter = new BinaryWriter(headerStream))
            {
                headerWriter.Write(0xC);
                headerWriter.Write(unicodeStreams.Count);
                headerWriter.Write(0x84);
                int startOffset = 28 + localization.Languages.Count * 24 + asciiBytes.Length;
                foreach(var lang in localization.Languages)
                {
                    headerWriter.Write(Encoding.ASCII.GetBytes(lang.Name));
                    headerWriter.Write(new byte[14]);
                    headerWriter.Write(startOffset);
                    headerWriter.Write(unicodeStreams[lang].Length);
                    startOffset += unicodeStreams[lang].Length;
                }
                headerWriter.Write(asciiBytes);
                writer.Write((int)headerStream.Length);
                writer.Write(headerStream.ToArray());
            }
            foreach (var lang in localization.Languages)
            {
                writer.Write(unicodeStreams[lang]);
            }
        }


        return stream.ToArray();
    }
    public static Dictionary<Language, byte[]> GetUnicodeLanguageStreams(IEnumerable<Language> languages, IEnumerable<LocString> locStrings)
    {
        Dictionary<Language, byte[]> languageStreams = new Dictionary<Language, byte[]>();
        int langOffset = locStrings.Count() * 8;
        foreach (var language in languages)
        {
            var langStream = new MemoryStream();
            using (var langWriter = new BinaryWriter(langStream))
            {
                langWriter.Write(langOffset / 8);
                langWriter.Write(8);
                using var langTextStream = new MemoryStream();
                using (var writer = new BinaryWriter(langTextStream))
                {
                    foreach (var item in locStrings.OrderBy(l => l.Id))
                    {
                        if (item.Texts.TryGetValue(language, out var text))
                        {
                            langWriter.Write(item.Id);
                            langWriter.Write((int)langTextStream.Position + langOffset + 8);
                            writer.Write(Encoding.Unicode.GetBytes(text.Text));
                            writer.Write((short)0);
                        }
                    }
                }
                langWriter.Write(langTextStream.ToArray());
            }
            languageStreams[language] = langStream.ToArray();
        }
        return languageStreams;
    }
    [DebuggerDisplay("{Header} - {Id}")]
    public class LocString
    {
        public uint Id { get; set; }
        public string Header { get; set; } = "";
        public Dictionary<Language, WrappedString> Texts { get; } = new Dictionary<Language, WrappedString>();
    }
    public class WrappedString
    {
        public string Text { get; set; } = "";

        public static implicit operator WrappedString(string text) => new WrappedString() { Text = text };
    }
    public class Language
    {
        public string Name { get; set; } = "";
    }
    public class LanguageHeader : IReadable
    {
        public string Name { get; set; } = "";
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint Start { get; set; }
        public uint End { get; set; }

        public void Read(ref Reader reader)
        {
            Name = reader.ReadString(4)[0..2];
            Unknown1 = reader.ReadUInt();
            Unknown2 = reader.ReadUInt();
            Unknown3 = reader.ReadUInt();
            Start = reader.ReadUInt();
            End = reader.ReadUInt();
        }
    }
}
