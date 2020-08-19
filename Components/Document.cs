using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

namespace YMTEditor
{
    public class Document
    {
        private Boolean isLoadProcess { get; set; }
        private OnDataLoaded _onDataLoaded { get; set; }
        private OnSetCompValue _onSetAvaliableComp { get; set; }
        private OnSetAvailableDrawable _onSetAvailableDrawable { get; set; }
        private OnSetAnchor _onSetAnchor { get; set; }
        private XmlNode root { get; set; }
        private OnRefreshXML _onRefreshXML { get; set; } = null;
        private XmlDocument doc { get; set; }
        public ComponentData componentData;

        public AnchorData anchorData;
        
        private XmlNode AvailCompNode()
        {
            //XmlNode root = doc.SelectSingleNode(".//" + "CPedVariationInfo");
            if (root.HasChildNodes)
                return root.SelectSingleNode(".//" + "availComp");
            return null;
        }

        private XmlNode ComponentDataNode()
        {
            //XmlNode root = doc.SelectSingleNode(".//" + "CPedVariationInfo");
            if (root.HasChildNodes)
                return root.SelectSingleNode(".//" + "aComponentData3");
            return null;
        }

        private XmlNode propInfoNode()
        {
            //XmlNode root = doc.SelectSingleNode(".//" + "CPedVariationInfo");
            if (root.HasChildNodes)
                return root.SelectSingleNode(".//" + "propInfo");
            return null;
        }

        private XmlNode compInfosNode()
        {
            //XmlNode root = doc.SelectSingleNode(".//" + "CPedVariationInfo");
            if (root.HasChildNodes)
                return root.SelectSingleNode(".//" + "compInfos");
            return null;
        }

        public Document()
        {
            //            Components = new List<Component>();
            //            Anchors = new List<Anchor>();
            componentData = new ComponentData();
            anchorData = new AnchorData();
            doc = new XmlDocument();
        }

        public void Init(OnDataLoaded onDataLoaded, OnSetCompValue onSetAvaliableComp,
             OnSetAvailableDrawable onSetAvailableDrawable, OnSetAnchor onSetAnchor, OnRefreshXML onRefreshXML)
        {
            _onDataLoaded = onDataLoaded;
            _onSetAvaliableComp = onSetAvaliableComp;
            _onSetAvailableDrawable = onSetAvailableDrawable;
            _onSetAnchor = onSetAnchor;
            _onRefreshXML = onRefreshXML;
        }

        private void RefreshXml()
        {
            if (_onRefreshXML != null) {
                _onRefreshXML(doc.InnerXml);
            }
        }

        public void Load(string FilePath)
        {
            isLoadProcess = true;
            XmlTextReader reader = new XmlTextReader(FilePath);
            reader.Read();
            doc.Load(reader);
            _onDataLoaded();
            GetStateFromDoc();
            isLoadProcess = false;
            RefreshXml();
        }

        public void LoadStream(Stream stream) 
        {
            isLoadProcess = true;
            doc.Load(stream);
            _onDataLoaded();
            GetStateFromDoc();
            isLoadProcess = false;
        }

        private Boolean IsLoadProcess() 
        {
            return isLoadProcess;
        }

        public void Save(string FilePath)
        {
            doc.Save(FilePath);
        }

        private void RemoveAnchorPropertyByIdx(int AnchorID, int Idx)
        {
            anchorData.RemoveAnchorPropertyByIdx(AnchorID, Idx);
        }
        
        private void GetStateFromDoc()
        {
            root = doc.SelectSingleNode(".//" + "CPedVariationInfo");
            componentData.Init(doc, AvailCompNode(), ComponentDataNode(), compInfosNode(), _onSetAvaliableComp, _onSetAvailableDrawable);
            anchorData.Init(propInfoNode(), _onSetAnchor, AddAnchorPropertyTexEvnt, AnchorPropertyAddEvnt, RemoveAnchorPropertyXML, AnchorAddEvnt, IsLoadProcess);
        }

        private void AddAnchorPropertyTexEvnt(int TexId, XmlNode texData) 
        {
            XmlElement elemRoot = (XmlElement)texData;
            XmlElement elemItem = doc.CreateElement("Item");
            elemRoot.AppendChild(elemItem);
            XmlElement elemIncl = doc.CreateElement("inclusions");
            elemIncl.InnerText = "0";
            elemItem.AppendChild(elemIncl);
            XmlElement elemExcl = doc.CreateElement("exclusions");
            elemExcl.InnerText = "0";
            elemItem.AppendChild(elemExcl);
            XmlElement elemTexId = doc.CreateElement("texId");
            elemTexId.SetAttribute("value", TexId.ToString());
            elemItem.AppendChild(elemTexId);
            XmlElement elemInclId = doc.CreateElement("inclusionId");
            elemInclId.SetAttribute("value", "0");
            elemItem.AppendChild(elemInclId);
            XmlElement elemExclId = doc.CreateElement("exclusionId");
            elemExclId.SetAttribute("value", "0");
            elemItem.AppendChild(elemExclId);
            XmlElement elemDistr = doc.CreateElement("distribution");
            elemDistr.SetAttribute("value", "255"); 
            elemItem.AppendChild(elemDistr);

        }

        private void SetAvaliableComp(int CompNumb, int val, Boolean Included)
        {
            componentData.SetAvaliableComp(CompNumb, val, Included);
        }

        public void NumTextureChanged(int ComponentID, int DrawableID, int Value, int TexId)
        {
            componentData.NumTextureChanged(ComponentID, DrawableID, Value, TexId);
            RefreshXml();
        }

        public void MaskChanged(int ComponentID, int DrawableID, int Value)
        {
            componentData.MaskChanged(ComponentID, DrawableID, Value);
            RefreshXml();
        }

        public void TextIDChanged(int ComponentID, int DrawableID, string Value)
        {
            componentData.TextIDChanged(ComponentID, DrawableID, Value);
            RefreshXml();
        }

        public void DrawableAdded(int ComponentID) 
        {
            componentData.DrawableAdded(ComponentID);
            RefreshXml();
        }
        public void DrawableDeleted(int ComponentID) 
        {
            componentData.DrawableDeleted(ComponentID);
            RefreshXml();
        }

        public void ComponentSwitched(int ComponentID, Boolean Value)
        {
            componentData.ComponentSwitched(ComponentID, Value);
            RefreshXml();
        }

        private XmlNode AnchorAddEvnt(int AnchorID) 
        {
            XmlNode propInfo = propInfoNode();
            if (propInfo == null)
                return null;
            foreach (XmlNode Anchors in propInfo.SelectNodes("aAnchors"))
            {
                XmlElement elemAnchorItem = doc.CreateElement("Item");
                XmlElement elemprops = doc.CreateElement("props");
                elemAnchorItem.AppendChild(elemprops);
                XmlElement elemAnchor = doc.CreateElement("anchor");
                elemAnchorItem.AppendChild(elemAnchor);
                XmlElement elemAnchors = (XmlElement)Anchors;
                elemAnchors.AppendChild(elemAnchorItem);
                return elemAnchorItem;
            }
            return null;
        }
        public void AnchorAdded(int AnchorID) 
        {
            anchorData.AddAnchor(AnchorID);
            RefreshXml();
        }

        public void AnchorRemove(int AnchorID, string anchorType)
        {
            anchorData.RemoveAnchor(AnchorID, anchorType);
            RefreshXml();
        }

        private XmlNode AnchorPropertyAddEvnt(int AnchorID, int PropertyId) 
        {
            XmlNode propInfo = propInfoNode();
            if (propInfo == null)
                return null;
            int Idx = -1;
            foreach (XmlNode Anchors in propInfo.SelectNodes("aAnchors"))
                foreach (XmlNode Anchor in Anchors.SelectNodes("Item"))
                    if (++Idx == AnchorID) 
                    {
                        foreach (XmlNode props in Anchor.SelectNodes("props"))
                            if (props.InnerText == "")
                                props.InnerText = "0";
                            else
                                props.InnerText += " 0";
                        foreach (XmlNode numAvail in propInfo.SelectNodes("numAvailProps")) 
                        {
                            XmlElement elemnumAvail = (XmlElement)numAvail;
                            elemnumAvail.SetAttribute("value", (Convert.ToInt32(elemnumAvail.GetAttribute("value"))+1).ToString()); 
                        }

                        foreach (XmlNode pmd in propInfo.SelectNodes("aPropMetaData")) 
                        {
                            XmlNode NextItem = null;
                            foreach (XmlNode item in pmd.SelectNodes("Item")) 
                            {
                                foreach (XmlNode anchorId in item.SelectNodes("anchorId"))
                                {
                                    XmlElement elemanchorId = (XmlElement)anchorId;
                                    if (Convert.ToInt32(elemanchorId.GetAttribute("value")) > AnchorID) 
                                    {
                                        NextItem = item;
                                        break;
                                    }
                                }
                            }

                            XmlElement elemItem = doc.CreateElement("Item");
                            XmlElement elemaudioId = doc.CreateElement("audioId");
                            elemItem.AppendChild(elemaudioId);
                            elemaudioId.InnerText = "none";
                            XmlElement elemexpressionMods = doc.CreateElement("expressionMods");
                            elemItem.AppendChild(elemexpressionMods);
                            elemexpressionMods.SetAttribute("value", "0 0 0 0 0");
                            XmlElement elemTexData = doc.CreateElement("texData");
                            elemItem.AppendChild(elemTexData);
                            elemTexData.SetAttribute("itemType", "CPedPropTexData");
                            XmlElement elemrenderFlags = doc.CreateElement("renderFlags");
                            elemItem.AppendChild(elemrenderFlags);
                            XmlElement elempropFlags = doc.CreateElement("propFlags");
                            elemItem.AppendChild(elempropFlags);
                            elempropFlags.SetAttribute("value", "0");
                            XmlElement elemflags = doc.CreateElement("flags");
                            elemItem.AppendChild(elemflags);
                            elemflags.SetAttribute("value", "0");
                            XmlElement elem_anchorId = doc.CreateElement("anchorId");
                            elemItem.AppendChild(elem_anchorId);
                            elem_anchorId.SetAttribute("value", AnchorID.ToString()); 
                            XmlElement elem_propId = doc.CreateElement("propId");
                            elemItem.AppendChild(elem_propId);
                            elem_propId.SetAttribute("value", PropertyId.ToString()); 
                            XmlElement elemhash = doc.CreateElement("hash_AC887A91");
                            elemItem.AppendChild(elemhash);
                            elemhash.SetAttribute("value", "0");
                            if (NextItem != null)
                                pmd.InsertBefore(elemItem, NextItem);
                            else
                                pmd.AppendChild(elemItem);
                            return elemItem;
                        }
                    }
            return null;
        }

        public void AnchorPropertyAdded(int AnchorID)
        {
            anchorData.AddAnchorProperty(AnchorID);
            RefreshXml();
        }

        public void AnchorPropertyRemoved(int AnchorID)
        {
            //anchorData.RemoveAnchor(AnchorID);
            RefreshXml();
        }

        private void RemoveAnchorPropertyXML(int AnchorId, int propertyIdx) 
        {
            XmlNode propInfo = propInfoNode();
            if (propInfo == null)
                return;
            int Idx = -1;
            foreach (XmlNode Anchors in propInfo.SelectNodes("aAnchors"))
                foreach (XmlNode Anchor in Anchors.SelectNodes("Item"))
                    if (++Idx == AnchorId)
                    {
                        foreach (XmlNode props in Anchor.SelectNodes("props"))
                        {
                            string propsstr = props.InnerText;
                            if (propsstr.Length > 2)
                                propsstr = propsstr.Substring(0, propsstr.Length - 2);
                            else
                                propsstr = "";
                            props.InnerText = propsstr;
                        }

                    }
            foreach (XmlNode numAvail in propInfo.SelectNodes("numAvailProps"))
            {
                XmlElement elemnumAvail = (XmlElement)numAvail;
                elemnumAvail.SetAttribute("value", (Convert.ToInt32(elemnumAvail.GetAttribute("value")) - 1).ToString());
            }

            foreach (XmlNode pmd in propInfo.SelectNodes("aPropMetaData"))
            {
                foreach (XmlNode item in pmd.SelectNodes("Item"))
                {
                    foreach (XmlNode anchorId in item.SelectNodes("anchorId"))
                    {
                        XmlElement elemanchorId = (XmlElement)anchorId;
                        foreach (XmlNode propId in item.SelectNodes("propId"))
                        {
                            XmlElement elempropId = (XmlElement)propId;
                            if ((Convert.ToInt32(elemanchorId.GetAttribute("value")) == AnchorId)
                                && (Convert.ToInt32(elempropId.GetAttribute("value")) == propertyIdx))
                            {
                                pmd.RemoveChild(item);
                                return;
                            }
                        }
                    }
                }

            }
        }
        public void RemoveAnchorProperty(int AnchorId, int propertyIdx)
        {
            RemoveAnchorPropertyByIdx(AnchorId, propertyIdx);
            RefreshXml();
        }

        public void AnchorTypeChanged(int AnchorID, string AnchorType) 
        {
            XmlNode propInfo = propInfoNode();
            if (propInfo == null)
                return;
            foreach (XmlNode Anchors in propInfo.SelectNodes("aAnchors"))
            {
                int Idx = -1;
                foreach (XmlNode Anchor in Anchors.SelectNodes("Item"))
                {
                    if (++Idx == AnchorID)
                    {
                        foreach (XmlNode anchor in Anchor.SelectNodes("anchor"))
                        {
                            anchor.InnerText = "ANCHOR_"+AnchorType;
                        }
                    }
                }   
            }
            RefreshXml();
        }

        public void AnchorPropNumTexturesChanged(int AnchorID, int PropID, int NumTextures) 
        {
            anchorData.AnchorPropNumTexturesChanged(AnchorID, PropID, NumTextures);
            RefreshXml();
        }
        
        public void AnchorPrfAlphaSwitched(int AnchorID, int PropID, Boolean Value) 
        {
            anchorData.AnchorPrfAlphaSwitched(AnchorID, PropID, Value);
            RefreshXml();
        }

        public void Clear() 
        {
           componentData.Clear();
           anchorData.Clear();
        }

        public XmlDocument getXmlDoc()
        {
            return doc;
        }
    }

}
