using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class PointerDataSource
{
	Dictionary<Type, object> data;
	public PointerDataSource(params object[] data)
	{
        this.data = new Dictionary<Type, object>();
		foreach (var item in data)
		{
			this.data[item.GetType()] = item;
		}
    }
	public bool TryGetData<T>([NotNullWhen(true)] out T? data)
	{
		if (this.data.TryGetValue(typeof(T), out var result))
		{
			data = (T)result;
			return true;
		}
		data = default;
		return false;
	}
}
