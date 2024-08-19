using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace BlurFormats.BlurData.Entities;
public class ExternalArrayEntity : IEntity, IArrayEntity, IExternalHydrateableEntity
{
    private readonly int type;
    private readonly int offset;
    private readonly int length;

    public ObservableCollection<ExternalArrayEntityItem> Value { get; }

    object IEntity.Value => Value;

    public Guid Guid { get; }

    public DataType Type { get; }

    public ExternalArrayEntity(DataType dataType)
    {
        Guid = Guid.NewGuid();
        Value = new ObservableCollection<ExternalArrayEntityItem>();
        Type = dataType;
    }
    public ExternalArrayEntity(DataType dataType, int type, int offset, int length)
    {
        Guid = Guid.NewGuid();
        Value = new ObservableCollection<ExternalArrayEntityItem>();
        Type = dataType;
        this.type = type;
        this.offset = offset;
        this.length = length;
    }
    public void AddEntity(ExternalReferenceEntity entity)
    {
        Value.Add(new ExternalArrayEntityItem(this, entity));
    }
    public void Hydrate(IReadOnlyList<BlurRecord> records, List<EntityBlock> blocks)
    {
        if (type == -1) return;
        var block = blocks[type];
        for (int i = offset; i < offset + length; i++)
        {
            var entity = block.Value[i];
            if (entity is ExternalForwardEntity f)
            {
                AddEntity(new ExternalReferenceEntity(Type) { Record = f.GetRecord(records) });
            }
            else
            {

            }
        }
    }
    public override string ToString() => $"{Type.Name}[] : {Value}";
}
