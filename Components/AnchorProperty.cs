using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YMTEditor
{
    public class AnchorProperty
    {
        private int FNumTextures = 0;
        private bool FIsPrfAlpha = false;
        private XmlNode PropertyNode { get; set; } = null;
        private IsLoadProcessDelegate _isLoadProcess { get; set; } = null;
        private OnAddAnchorPropertyTex _onAddAnchorPropertyTex { get; set; } = null;
        public int ID { get; set; }
        public int NumTextures
        {
            get
            {
                if (PropertyNode != null)
                {
                    XmlNode texData = PropertyNode.SelectSingleNode(".//" + "texData");
                    int cnt = 0;
                    foreach (XmlNode tex_item in texData.ChildNodes)
                    {
                        if (tex_item.Name == "Item")
                            cnt++;
                    }
                    FNumTextures = cnt;
                }
                return FNumTextures;
            }
            set
            {
                if ((PropertyNode != null) && (_isLoadProcess != null) && (_isLoadProcess() != true))
                {
                    XmlNode texData = PropertyNode.SelectSingleNode(".//" + "texData");
                    texData.RemoveAll();
                    XmlElement element = (XmlElement)texData;
                    element.SetAttribute("itemType", "CPedPropTexData");
                    for (int i = 0; i < value; i++)
                    {
                        _onAddAnchorPropertyTex(i, texData);
                    }
                }
                FNumTextures = value;
            }
        }
        public bool IsPrfAlpha
        {
            get
            {
                if (PropertyNode != null)
                {
                    XmlNode render = PropertyNode.SelectSingleNode(".//" + "renderFlags");
                    FIsPrfAlpha = render.InnerText != "";
                }
                return FIsPrfAlpha;
            }
            set
            {
                if ((PropertyNode != null) && (_isLoadProcess != null) && (_isLoadProcess() != true))
                {
                    XmlNode render = PropertyNode.SelectSingleNode(".//" + "renderFlags");

                    render.InnerText = value ? "PRF_ALPHA" : "";
                }
                FIsPrfAlpha = value;
            }
        }

        public void Init(XmlNode PropertyXml, OnAddAnchorPropertyTex AddAnchorPropertyTexEvnt,
            IsLoadProcessDelegate isLoadProcess)
        {
            PropertyNode = PropertyXml;
            if (PropertyNode == null)
                return;

            _onAddAnchorPropertyTex = AddAnchorPropertyTexEvnt;
            _isLoadProcess = isLoadProcess;

            XmlNode texData = PropertyNode.SelectSingleNode(".//" + "texData");
            int cnt = 0;
            foreach (XmlNode tex_item in texData.ChildNodes)
            {
                if (tex_item.Name == "Item")
                    cnt++;
            }
            XmlNode render = PropertyNode.SelectSingleNode(".//" + "renderFlags");
            Boolean Render = render != null ? render.InnerText != "" : false;
            XmlNode PropId = PropertyNode.SelectSingleNode(".//" + "propId");
            if (PropId != null)
            {
                XmlElement element2 = (XmlElement)PropId;
                int propId = PropId != null ? Convert.ToInt32(element2.GetAttributeNode("value").InnerXml) : -1;

                ID = propId;
                NumTextures = cnt;
                IsPrfAlpha = Render;
            }
        }
    }
}
