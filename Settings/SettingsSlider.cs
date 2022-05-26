using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
	public class SettingsSlider
    {
		const int iSLIDER_PANEL_HEIGHT = 30;
		const int iSLIDER_PANEL_WIDTH = 550;
		const int iSLIDER_LABEL_WIDTH = 450;

		private UISlider? m_slider = null;
		private UILabel? m_label = null;
		private string m_sText = "";
		private float m_fValue;
		private ICities.OnValueChanged? m_eventCallback = null;

		public static SettingsSlider Create(UIHelper helper, string sText, float fMin, float fMax, float fStep, float fDefault, ICities.OnValueChanged eventCallback)
		{
			
			SettingsSlider oSlider = new SettingsSlider();
			oSlider.m_fValue = fDefault;
			oSlider.m_slider = (UISlider) helper.AddSlider(sText, fMin, fMax, fStep, fDefault, oSlider.OnSliderValueChanged);
			oSlider.m_eventCallback = eventCallback;
			oSlider.m_sText = sText;
			UIPanel pnlParent = (UIPanel)oSlider.m_slider.parent;
			pnlParent.autoLayoutDirection = LayoutDirection.Horizontal;
			pnlParent.autoSize = false;
			pnlParent.width = iSLIDER_PANEL_WIDTH;
			pnlParent.height = iSLIDER_PANEL_HEIGHT;
			oSlider.m_slider.AlignTo(pnlParent, UIAlignAnchor.TopRight);
			oSlider.m_label = (UILabel)pnlParent.components[0];
			if (oSlider.m_label != null)
			{
				oSlider.m_label.autoSize = false;
				oSlider.m_label.width = iSLIDER_LABEL_WIDTH;
				oSlider.m_label.text = oSlider.m_sText + ": " + fDefault;
			}

			return oSlider;
		}

		public void OnSliderValueChanged(float fValue)
        {
			m_fValue = fValue;
			if (m_label != null)
            {
				m_label.text = m_sText + ": " + m_fValue;
			}
			if (m_eventCallback != null)
            {
				m_eventCallback(fValue);
			}
		}

		public void Enable(bool bEnable)
		{
			if (m_label != null)
            {
				m_label.isEnabled = bEnable;
				m_label.disabledTextColor = Color.grey;
			}
			if (m_slider != null)
            {
				m_slider.isEnabled = bEnable;
			}
		}
	}

    
}
