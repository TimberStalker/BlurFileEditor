using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Entities;
public static class EntityExtensions
{
    public static IEntity? GetField(this IEntity entity, string fieldName)
    {
        if(entity is StructureEntity s)
        {
            return s.GetPairdValues.FirstOrDefault(f => f.Field.Name == fieldName)?.Entity;
        }
        return null;
    }
    public static T? GetField<T>(this IEntity entity, string fieldName, T? defaultValue = default)
    {
        if(entity is StructureEntity s)
        {
            var fieldEntity = s.GetField(fieldName);
            if(fieldEntity is PrimitiveEntity p) return GetValue<T>(p);
        }
        return defaultValue;
    }
    public static T? GetValue<T>(this IEntity entity, T? defaultValue = default)
    {
        if(entity is PrimitiveEntity s) 
        {
            if (s.Value is T t) return t;
        }
        return defaultValue;
    }
}
