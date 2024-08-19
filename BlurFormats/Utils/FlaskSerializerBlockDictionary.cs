using BlurFormats.Serialization.Entities;
using BlurFormats.Serialization.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public class FlaskSerializerBlockDictionary : IEnumerable<(SerializationType, PointerTypes, List<IEntity>)>
{
    List<SerializationType> keys = new List<SerializationType>();
    List<List<(PointerTypes, List<IEntity>)>> values = [];

    public List<IEntity> this[(SerializationType, PointerTypes) key]
    {
        get => GetOrAdd(key);
    }
    public int Count => keys.Count;

    public bool IsReadOnly => false;

    public List<IEntity> GetOrAdd((SerializationType, PointerTypes) key)
    {
        var index = keys.IndexOf(key.Item1);
        if (index == -1)
        {
            keys.Add(key.Item1);
            List<IEntity> entities = [];
            values.Add([(key.Item2, entities)]);
            return entities;
        }
        else
        {
            var entityGroups = values[index];
            var entities = entityGroups!.FirstOrDefault(v => v.Item1 == key.Item2, ((PointerTypes)(-1), null)).Item2;
            if(entities == null)
            {
                entities = [];
                entityGroups.Add((key.Item2, entities));
            }
            return entities;
        }
    }
    public int BlockIndexOf(IEntity entity)
    {
        int index = 0;
        for(int i = 0; i < keys.Count; i++)
        {
            var value = values[i];
            for(int j = 0; j < value.Count; j++)
            {
                var (_, entities) = value[j];
                for (int k = 0; k < entities.Count; k++)
                {
                    var foundentity = entities[k];
                    if(foundentity == entity)
                    {
                        return index;
                    }
                }
                index++;
            }
            index++;
        }
        return -1;
    }
    public bool Contains(IEntity entity) => values.Any(v => v.Any(p => p.Item2.Contains(entity)));

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public IEnumerator<(SerializationType, PointerTypes, List<IEntity>)> GetEnumerator()
    {
        for(int i = 0; i < keys.Count; i++)
        {
            foreach (var (pointer, entities) in values[i].OrderByDescending(v => (int)v.Item1))
            {
                yield return (keys[i], pointer, entities);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
