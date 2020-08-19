using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Diagnostics;

namespace YMTEditor
{
    public class Component
    {
        private int FSTATEID = -1;
        private OnCreateDocElement _onCreateDocElement { get; set; } = null;
        private OnSetAvailableDrawable _onSetAvailableDrawable { get; set; } = null;
        private XmlNode componentData { get; set; } = null;
        private XmlNode compInfoNode { get; set; } = null;
        public int ID { get; set; }
        public int STATEID
        {
            get
            {
                return FSTATEID;
            }
            set
            {
                if (FSTATEID != value)
                    PREVID = FSTATEID;
                FSTATEID = value;
            }
        }
        public int PREVID { get; set; } = -1;
        public bool IsIncluded { get; set; }

        public List<Drawable> Drawables;

        public Drawable DrawableAdd(int Id, int numTextures, int mask, int texId, XmlNode ItemNode)
        {
            Drawable dr = new Drawable(Id, numTextures, mask, texId);
            dr.Init(ItemNode, RecalculateNumTextures, _onCreateDocElement);
            Drawables.Add(dr);
            return dr;
        }
        public void RemoveDrawable() { }

        public Component(int Id, int StateID, bool isIncluded)
        {
            ID = Id;
            STATEID = StateID;
            IsIncluded = isIncluded;
            Drawables = new List<Drawable>();
        }

        public void Init(OnSetAvailableDrawable onSetAvailableDrawable, OnCreateDocElement onCreateDocElement) 
        {
            _onSetAvailableDrawable = onSetAvailableDrawable;
            _onCreateDocElement = onCreateDocElement;
        }

        public int DeleteLastDrawable()
        {
            //Drawables.Remove(Drawables[Drawables.Count-1]);
            return Drawables.Count;
        }

        public int AvailTex()
        {
            int res = 0;
            foreach (Drawable dr in Drawables)
            {
                res += dr.NumTextures;
            }
            return res;
        }

        public void FillData(XmlNode ComponentData, XmlNode CompInfoNode) 
        {
            componentData = ComponentData;
            compInfoNode = CompInfoNode;

            XmlNode DrawblData = ComponentData.SelectSingleNode(".//" + "aDrawblData3");
            XmlNode DrawblItem;
            int DrawableId = 0;
            for (int j = 0; j < DrawblData.ChildNodes.Count; j++)
            {
                DrawblItem = DrawblData.ChildNodes[j];
                if (DrawblItem.Name == "Item")
                {
                    XmlNode prop = DrawblItem.SelectSingleNode(".//" + "propMask");
                    XmlElement element = (XmlElement)prop;
                    int propMask = prop != null ? Convert.ToInt32(element.GetAttributeNode("value").InnerXml) : 1;
                    XmlNode TexData = DrawblItem.SelectSingleNode(".//" + "aTexData");
                    int NumTex = 0;
                    foreach (XmlNode texNode in TexData.ChildNodes)
                        if (texNode.Name == "Item")
                            NumTex++;
                    XmlNode tdi = TexData.SelectSingleNode(".//" + "Item");
                    int TexId = 0;
                    if (tdi != null)
                    {
                        XmlNode texid = tdi.SelectSingleNode(".//" + "texId");
                        XmlElement element2 = (XmlElement)texid;
                        TexId = texid != null ? Convert.ToInt32(element2.GetAttributeNode("value").InnerXml) : 1;
                    }

                    SetAvailableDrawable(DrawableId, NumTex, propMask, TexId, DrawblItem);
                    DrawableId++;
                }
            }
        }

        public void TextIDChanged(int DrawableID, string Value)
        {
            Drawable dr = DrawableByID(DrawableID);
            if (dr != null)
                dr.TextIDChanged(Value);
        }

        public void MaskChanged(int DrawableID, int Value)
        {
            Drawable dr = DrawableByID(DrawableID);
            if (dr != null)
                dr.MaskChanged(Value);
        }

        public void NumTextureChanged(int DrawableID, int Value, int TexId)
        {
            Drawable dr = DrawableByID(DrawableID);
            if (dr != null)
                dr.NumTextureChanged(Value, TexId);
        }

        public void OnEnable() 
        {
//            DrawableAdded();
        }

        public void DrawableAdded()
        {

            XmlElement elemDrawblItem = null;
            foreach (XmlNode drawbl in componentData.SelectNodes("aDrawblData3"))
            {
                elemDrawblItem = _onCreateDocElement("Item");
                XmlElement elemPropMask = _onCreateDocElement("propMask");
                elemDrawblItem.AppendChild(elemPropMask);
                elemPropMask.SetAttribute("value", "0");
                XmlElement elemnumAlternatives = _onCreateDocElement("numAlternatives");
                elemDrawblItem.AppendChild(elemnumAlternatives);
                elemnumAlternatives.SetAttribute("value", "0");
                XmlElement elemTexData = _onCreateDocElement("aTexData");
                elemDrawblItem.AppendChild(elemTexData);
                elemTexData.SetAttribute("itemType", "CPVTextureData");
                XmlElement elemclothData = _onCreateDocElement("clothData");
                elemDrawblItem.AppendChild(elemclothData);
                XmlElement elemownsCloth = _onCreateDocElement("ownsCloth");
                elemclothData.AppendChild(elemownsCloth);
                elemownsCloth.SetAttribute("value", "false");
                drawbl.AppendChild(elemDrawblItem);
            }

            Drawable draw = DrawableAdd(Drawables.Count, 0, 0, 0, elemDrawblItem);

            int i = -1; int idx = -1;
            foreach (XmlNode node in compInfoNode.ChildNodes)
            {
                i++;
                foreach (XmlNode prop in node.SelectNodes("hash_D12F579D"))
                {
                    if (Convert.ToInt32(prop.Attributes["value"].Value) > ID)
                    {
                        idx = i;
                        break;
                    }
                    if (idx > -1)
                        break;
                }
                if (idx > -1)
                    break;
            }
            XmlNode res_node = null;
            if (idx == -1)
            {
                res_node = AddCompInfoNode(ID);
                compInfoNode.AppendChild(res_node);
            }
            else
            {
                res_node = AddCompInfoNode(ID);
                compInfoNode.InsertBefore(res_node, compInfoNode.ChildNodes[idx]);
            }
            foreach (XmlNode hash in res_node.SelectNodes("hash_FA1F27BF"))
            {
                XmlElement elemhash = (XmlElement)hash;
                elemhash.SetAttribute("value", draw.ID.ToString());
            }
        }
        
        public XmlNode AddCompInfoNode(int CompID)
        {
            XmlNode elemRoot = _onCreateDocElement("Item");
            XmlElement elemhash1 = _onCreateDocElement("hash_2FD08CEF");
            elemhash1.InnerText = "none";
            elemRoot.AppendChild(elemhash1);
            XmlElement elemhash2 = _onCreateDocElement("hash_FC507D28");
            elemhash2.InnerText = "none";
            elemRoot.AppendChild(elemhash2);
            XmlElement elemhash3 = _onCreateDocElement("hash_07AE529D");
            elemhash3.InnerText = "0 0 0 0 0";
            elemRoot.AppendChild(elemhash3);
            XmlElement elemFlags = _onCreateDocElement("flags");
            elemFlags.SetAttribute("value", "0");
            elemRoot.AppendChild(elemFlags);
            XmlElement elemIncl = _onCreateDocElement("inclusions");
            elemIncl.SetAttribute("value", "0");
            elemRoot.AppendChild(elemIncl);
            XmlElement elemExcl = _onCreateDocElement("exclusions");
            elemExcl.SetAttribute("value", "0");
            elemRoot.AppendChild(elemExcl);
            XmlElement elemhash4 = _onCreateDocElement("hash_6032815C");
            elemhash4.InnerText = "PV_COMP_HEAD";
            elemRoot.AppendChild(elemhash4);
            XmlElement elemhash5 = _onCreateDocElement("hash_7E103C8B");
            elemRoot.AppendChild(elemhash5);
            elemhash5.SetAttribute("value", "0");
            XmlElement elemhash6 = _onCreateDocElement("hash_D12F579D");
            elemRoot.AppendChild(elemhash6);
            elemhash6.SetAttribute("value", CompID.ToString());
            XmlElement elemhash7 = _onCreateDocElement("hash_FA1F27BF");
            elemhash7.SetAttribute("value", "0");
            elemRoot.AppendChild(elemhash7);

            return elemRoot;
        }






        private void RecalculateNumTextures() 
        {
            int NumTextures = 0;
            foreach (Drawable dr in Drawables)
                NumTextures += dr.NumTextures;
            XmlNode DrawblData = componentData.SelectSingleNode(".//" + "numAvailTex");
            XmlElement element = (XmlElement)DrawblData;
            element.SetAttribute("value", NumTextures.ToString());
        }

        private Drawable DrawableByID(int DrawableID) 
        {
            foreach (Drawable dr in Drawables)
                if (dr.ID == DrawableID)
                    return dr;
            return null;
        }

        private void SetAvailableDrawable(int DrawableID, int NumTex, int Mask, int TexId, XmlNode ItemNode)
        {
            if(_onSetAvailableDrawable != null)
                _onSetAvailableDrawable(ID, DrawableID, NumTex, Mask, TexId);
            DrawableAdd(DrawableID, NumTex, Mask, TexId, ItemNode);
        }
    }
}
