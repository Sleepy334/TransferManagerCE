using ColossalFramework.UI;
using System;
using System.Collections.Generic;

namespace SleepyCommon
{
	public abstract class ListData : IComparable
	{
		public abstract string GetText(ListViewRowComparer.Columns eColumn);
		public abstract int CompareTo(object second);
		public abstract void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns);
		public virtual void OnClick(ListViewRowColumn column)
		{

		}
		public virtual string OnTooltip(ListViewRowColumn column)
		{
			return "";
		}
		public virtual void Update()
        {

        }
	}
}