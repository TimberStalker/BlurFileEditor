﻿using BlurFormats.BlurData;
using BlurFormats.Loc;
using BlurFormats.Utils;
using System.Text;
using System.Xml;

var stream = File.Open(@"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\racingphysics.bin", FileMode.Open, FileAccess.Read);
var data = BlurData.Deserialize(stream);
;
//var originalData = File.ReadAllBytes(@"C:\Users\ChrisG\Desktop\BlurModified\gamedata\xt\amax.bin");
//var blurData = BlurData.Deserialize(originalData);
//var writer = XmlWriter.Create(Console.Out, new XmlWriterSettings()
//{
//    Indent = true,
//});
//blurData.WriteXml(writer);
//writer.Close();


//var filepath = "C:\\Users\\MyName\\Desktop\\BlurModified\\gamedata\\bklgi5q22\\bklgi5q22toc.bin";
//var filepath = "C:\\Users\\ChrisG\\Desktop\\BlurModified\\gamedata\\xt\\pickups.bin";
//var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "crowd.bin");
//if (filepath != null)
//{
    //var block = BinBlock.FromBytes(File.ReadAllBytes(filepath));
    //block.PrintTypes(Console.Out);
    //var loc = BlurData.Deserialize(File.ReadAllBytes(filepath));
    //loc.PrintTypes(Console.Out);
//}

//var loc = new Localization();
//loc.Languages.Add(new Localization.Language() { Name = "en" });
//loc.Languages.Add(new Localization.Language() { Name = "es" });
//loc.Languages.Add(new Localization.Language() { Name = "fr" });

//loc.Strings.Add(new Localization.LocString()
//{
//    Id = 934234,
//    Header = "Reference Header 1"
//});
//loc.Strings.Add(new Localization.LocString()
//{
//    Id = 992383,
//    Header = "Numero 2"
//});
//loc.Strings[0].Texts[loc.Languages[0]] = "Header_English";
//loc.Strings[0].Texts[loc.Languages[1]] = "Header_Spanish";
//loc.Strings[0].Texts[loc.Languages[2]] = "Header_French";

//loc.Strings[1].Texts[loc.Languages[0]] = "Double_English";
//loc.Strings[1].Texts[loc.Languages[1]] = "Double_Spanish";
//loc.Strings[1].Texts[loc.Languages[2]] = "Double_French";
//var filepath = "C:\\Users\\MyName\\Desktop\\BlurModified\\gamedata\\xt\\loc\\common.loc";
//var loc = Localization.Create(File.ReadAllBytes(filepath));
//var result = Localization.ToBytes(loc);
//File.WriteAllBytes("testencode.loc", result);