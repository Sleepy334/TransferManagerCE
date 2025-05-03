﻿using UnityEngine;
using ColossalFramework.UI;

using System;

namespace TransferManagerCE.UI
{
    public interface IUIFastListRow
    {
        #region Methods to implement
        /// <summary>
        /// Method invoked very often, make sure it is fast
        /// Avoid doing any calculations, the data should be already processed any ready to display.
        /// </summary>
        /// <param name="data">What needs to be displayed</param>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Display(int index, object data, bool isRowOdd);

        // Called when row transistions from having data to no data, can be used to clear tooltips etc...
        void Disabled();

        /// <summary>
        /// Change the style of the selected row here
        /// </summary>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Select(bool isRowOdd);
        /// <summary>
        /// Change the style of the row back from selected here
        /// </summary>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Deselect(bool isRowOdd);
        #endregion

        #region From UIPanel
        // No need to implement those, they are in UIPanel
        // Those are declared here so they can be used inside UIFastList
        float width { get; set; }
        bool enabled { get; set; }
        Vector3 relativePosition { get; set; }
        event MouseEventHandler eventClick;
        event MouseEventHandler eventMouseEnter;
        #endregion
    }

    /// <summary>
    /// This component is specifically designed the handle the display of
    /// very large amount of rows in a scrollable panel while minimizing
    /// the impact on the performances.
    /// 
    /// This class will instantiate the rows for you based on the actual
    /// height of the UIFastList and the rowHeight value provided.
    /// 
    /// The row class must inherit UIPanel and implement IUIFastListRow :
    /// public class MyCustomRow : UIPanel, IUIFastListRow
    /// 
    /// How it works :
    /// This class only instantiate as many rows as visible on screen (+1
    /// extra to simulate in-between steps). Then the content of those is
    /// updated according to what needs to be displayed by calling the
    /// Display method declared in IUIFastListRow.
    /// 
    /// Provide the list of data with rowData. This data is send back to
    /// your custom row when it needs to be displayed. For optimal
    /// performances, make sure this data is already processed and ready
    /// to display.
    /// 
    /// Creation example :
    /// UIFastList myFastList = UIFastList.Create<MyCustomRow>(this);
    /// myFastList.size = new Vector2(200f, 300f);
    /// myFastList.rowHeight = 40f;
    /// myFastList.rowData = myDataList;
    /// 
    /// </summary>
    public class UIFastList : UIComponent
    {
        private UIPanel m_panel;
        private UIScrollbar m_scrollbar;
        private FastList<IUIFastListRow> m_rows;
        private FastList<object> m_rowsData;

        private Type m_rowType;
        private string m_backgroundSprite;
        private Color32 m_color = new Color32(255, 255, 255, 255);
        private float m_rowHeight = -1;
        private float m_pos = -1;
        private float m_stepSize = 0;
        private bool m_canSelect = false;
        private int m_selectedDataId = -1;
        private int m_selectedRowId = -1;
        private bool m_lock = false;
        private bool m_updateContent = true;
        private bool m_autoHideScrollbar = false;
        private UIComponent m_lastMouseEnter;

        /// <summary>
        /// Use this to create the UIFastList.
        /// Do NOT use AddUIComponent.
        /// I had to do that way because MonoBehaviors classes cannot be generic
        /// </summary>
        /// <typeparam name="T">The type of the row UI component</typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static UIFastList Create<T>(UIComponent parent)
            where T : UIPanel, IUIFastListRow
        {
            UIFastList list = parent.AddUIComponent<UIFastList>();
            list.m_rowType = typeof(T);
            //list.backgroundSprite = "InfoviewPanel";
            //list.color = new Color32(255, 0, 0, 225);
            return list;
        }

        public bool autoHideScrollbar
        {
            get { return m_autoHideScrollbar; }
            set
            {
                if (m_autoHideScrollbar != value)
                {
                    m_autoHideScrollbar = value;
                    UpdateScrollbar();
                }
            }
        }
        /// <summary>
        /// Change the color of the background
        /// </summary>
        public Color32 backgroundColor
        {
            get { return m_color; }
            set
            {
                m_color = value;
                if (m_panel is not null)
                    m_panel.color = value;
            }
        }

        /// <summary>
        /// Change the sprite of the background
        /// </summary>
        public string backgroundSprite
        {
            get { return m_backgroundSprite; }
            set
            {
                if (m_backgroundSprite != value)
                {
                    m_backgroundSprite = value;
                    if (m_panel is not null)
                        m_panel.backgroundSprite = value;
                }
            }
        }

        /// <summary>
        /// Can rows be selected by clicking on them
        /// Default value is false
        /// Rows can still be selected via selectedIndex
        /// </summary>
        public bool canSelect
        {
            get { return m_canSelect; }
            set
            {
                if (m_canSelect != value)
                {
                    m_canSelect = value;

                    if (m_rows is null) return;
                    for (int i = 0; i < m_rows.m_size; i++)
                    {
                        if (m_canSelect)
                            m_rows[i].eventClick += OnRowClicked;
                        else
                            m_rows[i].eventClick -= OnRowClicked;
                    }
                }
            }
        }

        /// <summary>
        /// Change the position in the list
        /// Display the data at the position in the top row.
        /// This doesn't update the list if the position stay the same
        /// Use DisplayAt for that
        /// </summary>
        public float listPosition
        {
            get { return m_pos; }
            set
            {
                if (m_rowHeight <= 0) return;
                if (m_pos != value)
                {
                    float pos = Mathf.Max(Mathf.Min(value, m_rowsData.m_size - height / m_rowHeight), 0);
                    m_updateContent = Mathf.FloorToInt(m_pos) != Mathf.FloorToInt(pos);
                    DisplayAt(pos);
                }
            }
        }

        /// <summary>
        /// This is the list of data that will be send to the IUIFastListRow.Display method
        /// Changing this list will reset the display position to 0
        /// You can also change rowsData.m_buffer and rowsData.m_size
        /// and refresh the display with DisplayAt method
        /// </summary>
        public FastList<object> rowsData
        {
            get
            {
                if (m_rowsData is null)
                {
                    m_rowsData = new FastList<object>();
                }
                return m_rowsData;
            }
            set
            {
                if (m_rowsData is null)
                {
                    m_rowsData = value;
                    DisplayAt(0);
                }
                else if (m_rowsData != value)
                {
                    float pos = Mathf.Min(m_pos, value.m_size);
                    m_rowsData = value;
                    DisplayAt(pos);
                }
            }
        }

        /// <summary>
        /// This MUST be set, it is the height in pixels of each row
        /// </summary>
        public float rowHeight
        {
            get { return m_rowHeight; }
            set
            {
                if (m_rowHeight != value)
                {
                    m_rowHeight = value;
                    CheckRows();
                }
            }
        }

        /// <summary>
        /// Currently selected row
        /// -1 if none selected
        /// </summary>
        public int selectedIndex
        {
            get { return m_selectedDataId; }
            set
            {
                if (m_rowsData is null || m_rowsData.m_size == 0)
                {
                    m_selectedDataId = -1;
                    return;
                }

                int oldId = m_selectedDataId;
                if (oldId >= m_rowsData.m_size) oldId = -1;
                m_selectedDataId = Mathf.Min(Mathf.Max(-1, value), m_rowsData.m_size - 1);

                int pos = Mathf.FloorToInt(m_pos);
                int newRowId = Mathf.Max(-1, m_selectedDataId - pos);
                if (newRowId >= m_rows.m_size) newRowId = -1;

                if (newRowId >= 0 && newRowId == m_selectedRowId && !m_updateContent) return;

                if (m_selectedRowId >= 0)
                {
                    m_rows[m_selectedRowId].Deselect(oldId % 2 == 1);
                    m_selectedRowId = -1;
                }

                if (newRowId >= 0)
                {
                    m_selectedRowId = newRowId;
                    m_rows[m_selectedRowId].Select(m_selectedDataId % 2 == 1);
                }

                if (eventSelectedIndexChanged is not null && m_selectedDataId != oldId)
                    eventSelectedIndexChanged(this, m_selectedDataId);
            }
        }

        public object selectedItem
        {
            get
            {
                if (m_selectedDataId == -1) return null;
                return m_rowsData.m_buffer[m_selectedDataId];
            }
        }

        public bool selectOnMouseEnter
        { get; set; }

        /// <summary>
        /// The number of pixels moved at each scroll step
        /// When set to 0 or less, rowHeight is used instead.
        /// </summary>
        public float stepSize
        {
            get { return m_stepSize > 0 ? m_stepSize : m_rowHeight; }
            set { m_stepSize = value; }
        }

        /// <summary>
        /// Called when the currently selected row changed
        /// </summary>
        public event PropertyChangedEventHandler<int> eventSelectedIndexChanged;

        /// <summary>
        /// Clear the list
        /// </summary>

        public void Clear()
        {
            if (m_rowsData is not null)
            {
                m_rowsData.Clear();
            }

            if (m_rows is not null)
            {
                for (int i = 0; i < m_rows.m_size; i++)
                {
                    m_rows[i].enabled = false;
                }
            }

            UpdateScrollbar();
        }

        /// <summary>
        /// Display the data at the position in the top row.
        /// This update the list even if the position remind the same
        /// </summary>
        /// <param name="pos">Index position in the list</param>

        public void DisplayAt(float pos)
        {
            if (m_rowsData is null || m_rowHeight <= 0) return;

            SetupControls();

            m_pos = Mathf.Max(Mathf.Min(pos, m_rowsData.m_size - height / m_rowHeight), 0f);

            for (int i = 0; i < m_rows.m_size; i++)
            {
                int dataPos = Mathf.FloorToInt(m_pos + i);
                float offset = rowHeight * (m_pos + i - dataPos);

                if (dataPos < m_rowsData.m_size)
                {
                    if (m_updateContent)
                    {
                        m_rows[i].Display(i, m_rowsData[dataPos], dataPos % 2 == 1);
                    }
                        
                    if (dataPos == m_selectedDataId && m_updateContent)
                    {
                        m_selectedRowId = i;
                        m_rows[m_selectedRowId].Select(dataPos % 2 == 1);
                    }

                    m_rows[i].enabled = true;
                }
                else
                {
                    
                    if (m_rows[i].enabled)
                    {
                        m_rows[i].Disabled();
                    }

                    m_rows[i].enabled = false;
                }

                m_rows[i].relativePosition = new Vector3(0, i * rowHeight - offset);
            }

            UpdateScrollbar();
            m_updateContent = true;
        }

        /// <summary>
        /// Refresh the display
        /// </summary>

        public void Refresh()
        {
            DisplayAt(m_pos);
        }

        public override void Start()
        {
            base.Start();

            SetupControls();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_panel is null) return;

            m_panel.size = size;

            m_scrollbar.height = height;
            m_scrollbar.trackObject.height = height;
            m_scrollbar.AlignTo(this, UIAlignAnchor.TopRight);

            CheckRows();
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);

            if (rowsData is not null && rowsData.m_size > 0)
            {
                if (m_stepSize > 0 && m_rowHeight > 0)
                {
                    listPosition = m_pos - p.wheelDelta * m_stepSize / m_rowHeight;
                }
                else
                {
                    listPosition = m_pos - p.wheelDelta;
                }


                if (selectOnMouseEnter)
                {
                    OnRowClicked(m_lastMouseEnter, p);
                }
            }
        }

        protected void OnRowClicked(UIComponent component, UIMouseEventParameter p)
        {
            if (selectOnMouseEnter) m_lastMouseEnter = component;

            int max = Mathf.Min(m_rowsData.m_size, m_rows.m_size);
            for (int i = 0; i < max; i++)
            {
                if (component == (UIComponent)m_rows[i])
                {
                    selectedIndex = i + Mathf.FloorToInt(m_pos);
                    return;
                }
            }
        }

        private void CheckRows()
        {
            if (m_panel is null || m_rowHeight <= 0)
            {
                return;
            }

            int nbRows = Mathf.CeilToInt(height / m_rowHeight) + 1;

            if (m_rows is null)
            {
                m_rows = new FastList<IUIFastListRow>();
                m_rows.SetCapacity(nbRows);
            }

            if (m_rows.m_size < nbRows)
            {
                // Adding missing rows
                for (int i = m_rows.m_size; i < nbRows; i++)
                {
                    m_rows.Add(m_panel.AddUIComponent(m_rowType) as IUIFastListRow);

                    if (m_canSelect && !selectOnMouseEnter)
                    {
                        m_rows[i].eventClick += OnRowClicked;
                    }
                    else if (m_canSelect)
                    {
                        m_rows[i].eventMouseEnter += OnRowClicked;
                    }
                }
            }
            else if (m_rows.m_size > nbRows)
            {
                // Remove excess rows
                for (int i = nbRows; i < m_rows.m_size; i++)
                    Destroy(m_rows[i] as UnityEngine.Object);

                m_rows.SetCapacity(nbRows);
            }

            UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            if (m_rowsData is null || m_rowHeight <= 0) return;

            if (m_autoHideScrollbar)
            {
                bool isVisible = m_rowsData.m_size * m_rowHeight > height;
                float newPanelWidth = isVisible ? width - 10f : width;
                float newItemWidth = isVisible ? width - 20f : width;

                m_panel.width = newPanelWidth;
                for (int i = 0; i < m_rows.m_size; i++)
                {
                    m_rows[i].width = newItemWidth;
                }

                m_scrollbar.isVisible = isVisible;
            }
            else if (!m_scrollbar.isVisible)
            {
                // We hide the scroll bar till we can update it below
                m_scrollbar.isVisible = true;
            }

            //float H = m_rowHeight * m_rowsData.m_size;    (commenting out as never used)
            float scrollSize = height * height / (m_rowHeight * m_rowsData.m_size);
            float amount = stepSize * height / (m_rowHeight * m_rowsData.m_size);

            m_scrollbar.scrollSize = Mathf.Max(10f, scrollSize);
            m_scrollbar.minValue = 0f;
            m_scrollbar.maxValue = height;
            m_scrollbar.incrementAmount = Mathf.Max(1f, amount);

            UpdateScrollPosition();
        }

        private void UpdateScrollPosition()
        {
            if (m_lock || m_rowHeight <= 0) return;

            m_lock = true;
            try
            {
                float pos = m_pos * (height - m_scrollbar.scrollSize) / (m_rowsData.m_size - height / m_rowHeight);
                if (pos != m_scrollbar.value)
                {
                    m_scrollbar.value = pos;
                }
            }
            finally 
            { 
                m_lock = false; 
            }
        }


        private void SetupControls()
        {
            if (m_panel is not null)
            {
                return;
            }

            // Panel 
            m_panel = AddUIComponent<UIPanel>();
            m_panel.width = width - 10f;
            m_panel.height = height;
            m_panel.backgroundSprite = m_backgroundSprite;
            m_panel.color = m_color;
            m_panel.clipChildren = true;
            m_panel.relativePosition = Vector2.zero;

            // Scrollbar
            m_scrollbar = AddUIComponent<UIScrollbar>();
            m_scrollbar.width = 20f;
            m_scrollbar.height = height;
            m_scrollbar.orientation = UIOrientation.Vertical;
            m_scrollbar.pivot = UIPivotPoint.BottomLeft;
            m_scrollbar.AlignTo(this, UIAlignAnchor.TopRight);
            m_scrollbar.minValue = 0;
            m_scrollbar.value = 0;
            m_scrollbar.incrementAmount = 50;
            m_scrollbar.isVisible = false;

            UISlicedSprite tracSprite = m_scrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = tracSprite.parent.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";

            m_scrollbar.trackObject = tracSprite;

            UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";

            m_scrollbar.thumbObject = thumbSprite;

            // Rows
            CheckRows();

            m_scrollbar.eventValueChanged += (c, t) =>
            {
                if (m_lock || m_rowHeight <= 0) return;

                m_lock = true;
                try
                {
                    listPosition = m_scrollbar.value * (m_rowsData.m_size - height / m_rowHeight) / (height - m_scrollbar.scrollSize - 1f);
                }
                finally
                {
                    m_lock = false;
                }
            };
        }
    }
}
