using BlurFileFormats.Audio;
using Bufdio;
using Bufdio.Engines;
using Bufdio.Players;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Editor.Views.Dynamic;
public class BafEditorWindow : IDynamicView
{
    public string File { get; }
    public Baf Baf { get; }
    public string Name { get; }

    string IDynamicView.Id => File;

    string? IDynamicView.Shortcut => null;
    public BafEditorWindow(string file, Baf baf)
    {
        File = file;
        Baf = baf;
        Name = Path.GetFileName(file);
    }
    public bool Draw()
    {
        bool open = true;
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse;
        if (ImGui.Begin($"{Name}###{File}", ref open, flags))
        {
            ImGui.Text(Baf.Name);
            int i = 0;
            foreach (var track in Baf.Tracks)
            {
                ImGui.PushID(i);

                ImGui.Text(track.Name);
                if(ImGui.Button("Play"))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var options = new AudioEngineOptions(track.ChannelCount * Math.Max(track.TrackCount, 1), (int)track.SampleRate);
                            using var engine = new PortAudioEngine(options);
                            engine.Send(track.AudioStream.Select(c => c / (float)byte.MaxValue * short.MaxValue).ToArray());
                            await Task.Delay(2000);
                            ;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to play audio: {ex}");
                        }
                    });
                }
                ImGui.Text($"0.00s : {track.SampleCount/(float)track.SampleRate:0.00s}");

                ImGui.PopID();
                i++;
            }
            ImGui.End();
        }
        return open;
    }
}
