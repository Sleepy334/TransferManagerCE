using SleepyCommon;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    public abstract class CheckListData : IComparable
    {
        public CheckListData()
        {
        }

        public CheckListData(CheckListData oSecond)
        {
        }

        public abstract string GetText();

        public abstract bool IsChecked();

        public abstract void OnItemCheckChanged(bool bChecked);

        public abstract void OnShow();

        public virtual Color GetTextColor()
        {
            return KnownColor.white;
        }

        public virtual int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }
            CheckListData oSecond = (CheckListData)second;
            return GetText().CompareTo(oSecond.GetText());
        }
    }
}