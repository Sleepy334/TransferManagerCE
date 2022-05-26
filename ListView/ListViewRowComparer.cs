using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE;
using UnityEngine;

namespace SleepyCommon
{
	public class ListViewRowComparer : IComparer<UIComponent>
	{
		public enum Columns
        {
            COLUMN_INOUT,
			COLUMN_MATERIAL,
            COLUMN_AMOUNT,
			COLUMN_PRIORITY,
            COLUMN_DESCRIPTION,
			COLUMN_TIME,
			COLUMN_DELTAAMOUNT,
			COLUMN_IN_COUNT,
			COLUMN_IN_AMOUNT,
			COLUMN_OUT_COUNT,
			COLUMN_OUT_AMOUNT,
			COLUMN_MATCH_COUNT,
			COLUMN_MATCH_AMOUNT,
			COLUMN_IN_PERCENT,
			COLUMN_OUT_PERCENT,
			COLUMN_TARGET,
			COLUMN_VALUE,
			COLUMN_OWNER,
			COLUMN_VEHICLE,
		}
        
        public Columns m_eSortColumn;
		public bool m_bSortDesc;

		public ListViewRowComparer(Columns eSortColumn, bool bSortDesc)
        {
            m_eSortColumn = eSortColumn;
			m_bSortDesc = bSortDesc;
		}

		public int Compare(UIComponent o1, UIComponent o2)
		{
			ListViewRow? oRow1 = o1 as ListViewRow;
			ListViewRow? oRow2 = o2 as ListViewRow;

			int iResult = 1;
			if (oRow1 != null && oRow2 != null)
            {
                iResult = oRow1.CompareTo(oRow2);
				if (m_bSortDesc)
                {
					iResult = -iResult;
				}
			}
			return iResult;
		}
	}
}
