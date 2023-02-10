using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class PointerEntity : IEntity, IPointerEntity
{
    public int Pointer { get; }

    object IEntity.Value => Pointer;

    public Guid Guid { get; }

    public DataType Type { get; }

    public PointerEntity(DataType dataType, int value)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Pointer = value;
    }
    public override string ToString() => $"{Type.Name} : {Pointer}";

    public virtual IEntity GetReplacement(PointerDataSource dataSource)
    {
        if (dataSource.TryGetData<List<EntityBlock>>(out var entityBlock))
        {
            if (Pointer == -1) return new NullEntity(Type);
            return entityBlock[Pointer].Value[0];
        }
        return new NullEntity(Type);
    }
}
