using BlurFormats.BlurData.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class ArrayPointerEntity : IEntity, IPointerEntity
{
    public short Pointer { get; }
    public short Modifier { get; }
    public int Length { get; }
    public FieldType FieldType { get; }

    object IEntity.Value => (Pointer, Modifier, Length);

    public Guid Guid { get; }

    public DataType Type { get; }

    public ArrayPointerEntity(DataType dataType, short pointer, short modifier, int length, FieldType fieldType)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Pointer = pointer;
        Modifier = modifier;
        Length = length;
        FieldType = fieldType;
    }

    public virtual IEntity GetReplacement(PointerDataSource dataSource)
    {
        if (dataSource.TryGetData<List<EntityBlock>>(out var entityBlock))
        {
            var replacement = new ArrayEntity(Type);
            //for (int i = 0; i < Length; i++)
            //{
                //replacement.Value.Add(new ObjectEntityItem(null, entityBlock[Pointer].Value[i + Modifier]));
            //}
            return replacement;
        }
        return new NullEntity(Type);
    }
    public override string ToString()
    {
        return $"{FieldType}[{Length}] | {Pointer} : {Modifier}";
    }
}
