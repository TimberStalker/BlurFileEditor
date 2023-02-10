using BlurFormats.BlurData.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class DoublePointerEntity : IEntity, IPointerEntity
{
    public short Pointer1 { get; }
    public short Pointer2 { get; }

    object IEntity.Value => (Pointer1, Pointer2);

    public Guid Guid { get; }

    public DataType Type { get; }
    public DoublePointerEntity(DataType dataType, short pointer1, short pointer2)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Pointer1 = pointer1;
        Pointer2 = pointer2;
    }
    public override string ToString() => $"{Type.Name} : ({Pointer1} {Pointer2})";

    public IEntity GetReplacement(PointerDataSource dataSource)
    {
        if (dataSource.TryGetData<List<EntityBlock>>(out var entityBlock))
        {
            if(Pointer1 != -1)
                return entityBlock[Pointer1].Value[Pointer2];
        }
        return new NullEntity(Type);
    }
}
