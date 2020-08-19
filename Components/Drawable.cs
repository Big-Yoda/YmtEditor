using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YMTEditor
{
    public class Drawable
    {
        private XmlNode DrawableNode { get; set; } = null;
        private OnNumTexturesChangedDelegate onNumTexturesChanged { get; set; } = null;
        private OnCreateDocElement onCreateDocElement { get; set; } = null;

        private int FNumTextures = 0;
        public int ID { get; set; }
        public int NumTextures
        {
            get { return FNumTextures; }
            set
            {
                if (FNumTextures != value)
                {
                    FNumTextures = value;
                    if (onNumTexturesChanged != null)
                        onNumTexturesChanged();
                }
            }
        }

        public int Mask { get; set; }
        public int TexId { get; set; }

        public Drawable(int Id, int numTextures, int mask, int texId)
        {
            ID = Id;
            NumTextures = numTextures;
            Mask = mask;
            TexId = texId;
        }

        public void Init(XmlNode drawableNode, OnNumTexturesChangedDelegate OnNumTexturesChanged, OnCreateDocElement _onCreateDocElement) 
        {
            DrawableNode = drawableNode;
            onNumTexturesChanged = OnNumTexturesChanged;
            onCreateDocElement = _onCreateDocElement;
        }
        public void TextIDChanged(string Value) 
        {
            TexId = Convert.ToInt32(Value);

            if(DrawableNode != null)
                foreach (XmlNode tex in DrawableNode.SelectNodes("aTexData"))
                    foreach (XmlNode texItem in tex.SelectNodes("Item"))
                        foreach (XmlNode texIdItem in texItem.SelectNodes("texId"))
                        {
                            XmlElement elemtexId = (XmlElement)texIdItem;
                            elemtexId.SetAttribute("value", Value);
                        }
        }

        public void MaskChanged(int Value)
        {
            Mask = Value;
            if (DrawableNode != null)
                foreach (XmlNode mask in DrawableNode.SelectNodes("propMask"))
                    mask.Attributes["value"].Value = Value.ToString();
        }

        public void NumTextureChanged(int Value, int TexId)
        {
            NumTextures = Value;
            foreach (XmlNode tex in DrawableNode.SelectNodes("aTexData"))
            {
                if (onCreateDocElement == null)
                    return;
                tex.RemoveAll();
                XmlElement elemtex = (XmlElement)tex;
                elemtex.SetAttribute("itemType", "CPVTextureData");
                for (int i = 0; i < Value; i++)
                {
                    XmlElement elem_item = onCreateDocElement("Item");
                    elemtex.AppendChild(elem_item);
                    XmlElement elem_texId = onCreateDocElement("texId");
                    elem_item.AppendChild(elem_texId);
                    elem_texId.SetAttribute("value", TexId.ToString());

                    XmlElement elem_distr = onCreateDocElement("distribution");
                    elem_item.AppendChild(elem_distr);
                    elem_distr.SetAttribute("value", "255");
                }
            }
        }
    }
}
