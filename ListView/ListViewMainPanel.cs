using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace SleepyCommon
{
    public class ListViewMainPanel : UIScrollablePanel
    {
        public ListViewMainPanel() : base() 
        {
        }

        public void Setup(string sBackgroundStyle, float fWidth, float fHeight)
        {
            backgroundSprite = sBackgroundStyle;// "GenericPanelWhite";
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;
            width = fWidth;
            height = fHeight;
        }

        public void Clear()
        {
            if (components == null) 
            {
                return;
            }

            for (int index = 0; index < components.Count; ++index)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object) components[index].gameObject);
            }

            m_ChildComponents = PoolList<UIComponent>.Obtain();
        }

        public void Sort(ListViewRowComparer.Columns eSortColumn, bool bDesc)
        {
            m_ChildComponents.Sort(new ListViewRowComparer(eSortColumn, bDesc));
        }
    }
}
