using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

namespace YMTEditor
{
    public class Anchor
    {
        private OnSetAnchor _onSetAnchor { get; set; }
        private OnAddAnchorPropertyTex _onAddAnchorPropertyTex { get; set; }
        private OnAddAnchorPropertyXML _onAddAnchorPropertyXML { get; set; }
        private OnRemoveAnchorPropertyXML _onRemoveAnchorPropertyXML { get; set; }
        private IsLoadProcessDelegate _isLoadProcess { get; set; } = null;
        private XmlNode AnchorXml { get; set; }
        private XmlNode AnchorPropsXml { get; set; }
        private AnchorAddXMLEvnt _onAnchorAdd { get; set; }
        public int Id { get; set; }
        public int AnchorId { get; set; }
        public string AnchorName { get; set; }
        public List<AnchorProperty> Properties;

        public Anchor(int ID)
        {
            Id = ID;
            Properties = new List<AnchorProperty>();
        }

        public void Init(XmlNode AnchorNode, XmlNode AnchorProps, OnSetAnchor onSetAnchor, OnAddAnchorPropertyTex AddAnchorPropertyTexEvnt,
            OnAddAnchorPropertyXML onAddAnchorPropertyXML, OnRemoveAnchorPropertyXML onRemoveAnchorPropertyXML, IsLoadProcessDelegate isLoadProcess)
        {
            AnchorXml = AnchorNode;
            AnchorPropsXml = AnchorProps;
            _onSetAnchor = onSetAnchor;
            _onAddAnchorPropertyTex = AddAnchorPropertyTexEvnt;
            _onAddAnchorPropertyXML = onAddAnchorPropertyXML;
            _onRemoveAnchorPropertyXML = onRemoveAnchorPropertyXML;
            _isLoadProcess = isLoadProcess;

            Properties.Clear();
            XmlNode prop_node = AnchorXml.SelectSingleNode(".//" + "props");
            if (prop_node != null)
                ParseAnchorProps(prop_node.InnerText);
            XmlNode anchor_type = AnchorNode.SelectSingleNode(".//" + "anchor");
            AnchorName = AnchorNameFromAnchorTypes(anchor_type != null ? anchor_type.InnerText : "");
            for (int j = 0; j < AnchorPropsXml.ChildNodes.Count; j++)
            {
                XmlNode CurrProperty = AnchorProps.ChildNodes[j];
                if (CurrProperty.Name == "Item")
                {
                    XmlNode anchorId = CurrProperty.SelectSingleNode(".//" + "anchorId");
                    XmlElement element3 = (XmlElement)anchorId;
                    int anchorID = anchorId != null ? Convert.ToInt32(element3.GetAttributeNode("value").InnerXml) : -1;
                    if (anchorID == Id)
                    {
                        SetProperty(CurrProperty);
                    }
                }
            }
        }

        public Boolean SetPropValues(int PropID, int TextCount, bool IsPrfAlpha)
        {
            foreach (AnchorProperty prop in Properties)
            {
                if (prop.ID == PropID)
                {
                    prop.NumTextures = TextCount;
                    prop.IsPrfAlpha = IsPrfAlpha;
                    return true;
                }
            }
            return false;
        }

        public void LoadAnchorProp(int ID, int propValue)
        {
            AnchorProperty property = new AnchorProperty();
            property.ID = ID;
            property.NumTextures = propValue;
            property.Init(null, _onAddAnchorPropertyTex, _isLoadProcess);
            AddProperty(property);
        }
        public void AddAnchorProp(int ID, int propValue)
        {
            AnchorProperty property = new AnchorProperty();
            property.ID = ID;
            property.NumTextures = propValue;
            property.Init(_onAddAnchorPropertyXML(Id, ID), _onAddAnchorPropertyTex, _isLoadProcess);
            AddProperty(property);
        }

        public void RemoveAnchorPropertyByIdx(int AnchorIdx, int Idx)
        {
            foreach (AnchorProperty prop in Properties)
            {
                if (prop.ID == Idx)
                {
                    Properties.Remove(prop);
                    _onRemoveAnchorPropertyXML(AnchorIdx, Idx);
                    break;
                }
            }
        }

        private void SetProperty(XmlNode propertyNode)
        {
            AnchorProperty prop = new AnchorProperty();
            prop.Init(propertyNode, _onAddAnchorPropertyTex, _isLoadProcess);

            foreach (AnchorProperty p in Properties)
            {
                if (p.ID == prop.ID)
                {
                    p.Init(propertyNode, _onAddAnchorPropertyTex, _isLoadProcess);
                    return;
                }
            }
            AddProperty(propertyNode);
        }
        private void AddProperty(XmlNode propertyNode)
        {
            AnchorProperty prop = new AnchorProperty();
            prop.Init(propertyNode, _onAddAnchorPropertyTex, _isLoadProcess);
            Properties.Add(prop);
        }

        public int AddProperty(AnchorProperty property)
        {
            AnchorProperty prop = property;
            if (prop == null)
            {
                prop = new AnchorProperty();
                //                prop.Init(null, _onAddAnchorPropertyTex, _isLoadProcess);
                prop.Init(_onAddAnchorPropertyXML(Id, Properties.Count), _onAddAnchorPropertyTex, _isLoadProcess);
            }
            Properties.Add(prop);
            prop.ID = Properties.Count - 1;
            return Properties.Count - 1;
        }

        private string AnchorNameFromAnchorTypes(string AnchorType)
        {
            string prefix = "ANCHOR_";
            int pos = AnchorType.IndexOf(prefix);
            if (pos != -1)
            {
                return AnchorType.Substring(pos + prefix.Length);
            }
            return "";
        }

        private void ParseAnchorProps(string AnchorProps)
        {
            AnchorProps = Regex.Replace(AnchorProps, "  +", "", RegexOptions.Compiled);
            AnchorProps = AnchorProps.Replace("\n", "").Replace("\r", "");
            string[] anchorProps = AnchorProps.Split(' ');
            foreach (var num in anchorProps)
            {
                if (num == "" || num == " ") continue;
                LoadAnchorProp(Properties.Count, Convert.ToInt32(num));
            }
        }
        
        public void AnchorPropNumTexturesChanged(int PropID, int NumTextures)
        {
            foreach (AnchorProperty ap in Properties)
            {
                if (ap.ID == PropID)
                    ap.NumTextures = NumTextures;
            }
            string propStr = "";

            foreach (AnchorProperty ap in Properties)
            {
                if (propStr == "")
                    propStr = ap.NumTextures.ToString();
                else
                    propStr += " " + ap.NumTextures.ToString();
            }
            XmlNode prop_node = AnchorXml.SelectSingleNode(".//" + "props");
            prop_node.InnerText = propStr;
        }

        public void AnchorPrfAlphaSwitched(int PropID, Boolean Value)
        {
            foreach (AnchorProperty ap in Properties)
            {
                if (ap.ID == PropID)
                    ap.IsPrfAlpha = Value;
            }
        }

    }
}
