using BlurFormats.BlurData.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
public sealed class BlurRecord : IEnumerable<BlurRecord>
{
    public uint ID { get; set; }
	public IEntity Entity { get; set; }
    public RecordHeap Heap { get; }
	public DataType BaseType => Entity.Type;
	public BlurRecord(uint id, IEntity entity)
	{
		ID = id;
		Entity = entity;
		Heap = new RecordHeap();
    }
    public BlurRecord(uint id, IEntity entity, RecordHeap recordHeap)
    {
        ID = id;
        Entity = entity;
		Heap = recordHeap;
    }

    public IEnumerator<BlurRecord> GetEnumerator()
    {
        return new List<BlurRecord>{this}.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
public sealed class RecordHeap
{
    public IEntity this[int index] => this[index];

    List<IEntity> entities = new List<IEntity>();

    public IReadOnlyList<IEntity> Entities => entities;
	
	public void AddEntity(IEntity entity)
    {
        entities.Add(entity);
    }
    public void RemoveEntity(IEntity entity)
    {
        entities.Remove(entity);
    }
    public ReferenceEntity GetReference(IEntity entity)
    {
        var type = entity.Type;
        return new ReferenceEntity(type, this) { Reference = entity };
    }
}
