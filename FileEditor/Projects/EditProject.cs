using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlurFileFormats.FlaskReflection;
using BlurFileFormats.Localizations;
using BlurFileFormats.Localizations.Serialization;
using Language = BlurFileFormats.Localizations.Language;

namespace Editor.Projects;
public class Project
{
    public string BaseDirectory { get; }

    public FlaskData Flask { get; }
    public LocData Localizations { get; }
    Project(string baseDirectory)
    {
        this.BaseDirectory = baseDirectory;
        Flask = new(this);
        Localizations = new(this);
    }

    public static Project CreateEmpty()
    {
        return new Project(Environment.CurrentDirectory);
    }

    public static Project Open(string path)
    {
        var directory = Path.GetDirectoryName(path) ?? throw new ArgumentException($"'{path}' is not a valid path.", nameof(path));

        List<string> locFiles = [];
        List<string> flaskFiles = [];

        foreach(var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            switch (Path.GetExtension(file))
            {
                case ".loc":
                    locFiles.Add(Path.GetRelativePath(directory, file));
                    break;
                case ".bin":
                case ".xt":
                    flaskFiles.Add(Path.GetRelativePath(directory, file));
                    break;
            }
        }

        var project = new Project(directory);
        foreach (var item in flaskFiles)
        {
            _ = project.Flask.InitializeXtDatabase(item);
        }
        
        foreach (var item in locFiles)
        {
            _ = project.Localizations.GetLocalization(item);
        }

        return project;
    }

}
public class FlaskData
{
    object _lock = new();
    public XtDatabase GlobalXtDatabase { get; } = new();
    ConcurrentDictionary<string, XtDatabase> ScopedDatabases { get; } = [];
    ConcurrentDictionary<uint, string> RecordSourceMappings { get; } = [];
    ConcurrentBag<IXtType> types = [];
    Project Project { get; }
    public IEnumerable<IXtType> Types => types;
    public FlaskData(Project project)
    {
        Project = project;
    }
    public async Task InitializeXtDatabase(string file)
    {
        if (Path.IsPathFullyQualified(file) && file.StartsWith(Project.BaseDirectory))
        {
            file = Path.GetRelativePath(Project.BaseDirectory, file);
        }

        if (ScopedDatabases.ContainsKey(file)) return;
        await Task.Run(() =>
        {
            try
            {
                var scopedDatabase = Flask.Import(Path.Combine(Project.BaseDirectory, file));
            lock (_lock)
            {
                foreach (var (key, value) in scopedDatabase.Refs)
                {
                    GlobalXtDatabase.Refs[key] = value;
                    RecordSourceMappings[key] = file;
                }
                foreach (var type in scopedDatabase.Types)
                {
                    types.Add(type);
                }
            }
            ScopedDatabases.TryAdd(file, scopedDatabase);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load flask file {file}: {ex}");
        }
    });
    }
    public void AddRecord(string file, XtRef record)
    {
        GlobalXtDatabase.Refs[record.Handle] = record;
        RecordSourceMappings[record.Handle] = file;
    }
    public void RemoveRecord(XtRef record)
    {
        GlobalXtDatabase.Refs.Remove(record.Handle);
        RecordSourceMappings.Remove(record.Handle, out _);
    }
    public async Task<XtDatabase?> GetXtDatabase(string file)
    {
        if (Path.IsPathFullyQualified(file) && file.StartsWith(Project.BaseDirectory))
        {
            file = Path.GetRelativePath(Project.BaseDirectory, file);
        }

        await InitializeXtDatabase(file);

        return ScopedDatabases.TryGetValue(file, out var db) ? db : null;
    }
    public bool TryGetRef(uint id, [NotNullWhen(true)]out XtRef? reference)
    {
        return GlobalXtDatabase.Refs.TryGetValue(id, out reference);
    }
    public XtRef? GetRef(uint id) => TryGetRef(id, out var xtRef) ? xtRef : null;
    public string? GetRecordSourceFile(uint id)
    {
        if (RecordSourceMappings.TryGetValue(id, out var file))
        {
            return file;
        }
        return null;
    }
}

public class LocData
{
    public ConcurrentBag<Language> Languages { get; } = [];

    ConcurrentDictionary<string, Task<Localization>> ScopedLocalizations { get; } = [];
    public ConcurrentDictionary<uint, Text> TextMappings { get; } = [];
    Project Project { get; }

    public LocData(Project project)
    {
        Project = project;
    }
    public Task<Localization> GetLocalization(string file)
    {
        return ScopedLocalizations.GetOrAdd(file, f => Task.Run(() =>
        {
            var loc = LocalizationSerializer.DeserializeLocalization(Path.Combine(Project.BaseDirectory, file));
            lock (ScopedLocalizations)
            {
                foreach (var text in loc.Texts)
                {
                    TextMappings[text.Id] = text;
                }
            }
            return loc;
        }));
    }
    public Text? GetText(uint id)
    {
        if (TextMappings.TryGetValue(id, out var text))
        {
            return text;
        }
        return null;
    }
}