using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace YMTEditor
{
    public class AnchorData
    {
        private OnAddAnchorPropertyTex _onAddAnchorPropertyTex { get; set; } = null;
        private OnAddAnchorPropertyXML _onAddAnchorPropertyXML { get; set; } = null;
        private OnRemoveAnchorPropertyXML _onRemoveAnchorPropertyXML { get; set; } = null;
        private IsLoadProcessDelegate isLoadProcess { get; set; } = null;
        private OnSetAnchor _onSetAnchor { get; set; }
        private AnchorAddXMLEvnt _onAnchorAdd { get; set; }

        private XmlNode propinfo { get; set; }
        public List<Anchor> Anchors { get; set; }

        public AnchorData()
        {
            Anchors = new List<Anchor>();
        }

        public void Init(XmlNode PropInfo, OnSetAnchor onSetAnchor, OnAddAnchorPropertyTex AddAnchorPropertyTexEvnt,
            OnAddAnchorPropertyXML onAddAnchorPropertyXML, OnRemoveAnchorPropertyXML onRemoveAnchorPropertyXML, AnchorAddXMLEvnt onAnchorAdd,
            IsLoadProcessDelegate IsLoadProcess)
        {
            propinfo = PropInfo;
            _onSetAnchor = onSetAnchor;
            _onAddAnchorPropertyTex = AddAnchorPropertyTexEvnt;
            _onAddAnchorPropertyXML = onAddAnchorPropertyXML;
            _onRemoveAnchorPropertyXML = onRemoveAnchorPropertyXML;
            _onAnchorAdd = onAnchorAdd;
            isLoadProcess = IsLoadProcess;
            Anchors.Clear();

            if (propinfo != null && propinfo.HasChildNodes)
            {
                XmlNode aAnchors = propinfo.SelectSingleNode(".//" + "aAnchors");
                XmlNode AnchorItem;
                XmlNode AnchorProps = propinfo.SelectSingleNode(".//" + "aPropMetaData");
                int LoadedProps = 0;
                for (int i = 0; i < aAnchors.ChildNodes.Count; i++)
                {
                    AnchorItem = aAnchors.ChildNodes[i];
                    if (AnchorItem.Name == "Item")
                    {
                        Anchor anchor = new Anchor(Anchors.Count);
                        Anchors.Add(anchor);
                        anchor.Init(AnchorItem, AnchorProps, onSetAnchor, _onAddAnchorPropertyTex, _onAddAnchorPropertyXML, _onRemoveAnchorPropertyXML, IsLoadProcess);
                        LoadedProps += anchor.Properties.Count;
                        _onSetAnchor(anchor.Id, anchor);
                    }
                }
            }
        }

        public void Clear()
        {
            Anchors.Clear();
            if (propinfo == null)
                return;

            XmlNode aAnchors = propinfo.SelectSingleNode(".//" + "aAnchors");
            aAnchors.RemoveAll();
            XmlElement elemAnchors = (XmlElement)aAnchors;
            elemAnchors.SetAttribute("itemType", "CAnchorProps");

            XmlNode AnchorProps = propinfo.SelectSingleNode(".//" + "aPropMetaData");
            AnchorProps.RemoveAll();
            XmlElement elemAnchorProps = (XmlElement)AnchorProps;
            elemAnchorProps.SetAttribute("itemType", "CPedPropMetaData");

            foreach (XmlNode numAvail in propinfo.SelectNodes("numAvailProps"))
            {
                XmlElement elemnumAvail = (XmlElement)numAvail;
                elemnumAvail.SetAttribute("value", "0");
            }

        }

        public void AnchorPropNumTexturesChanged(int AnchorID, int PropID, int NumTextures)
        {
            foreach (Anchor anch in Anchors)
            {
                if (anch.Id == AnchorID)
                    anch.AnchorPropNumTexturesChanged(PropID, NumTextures);
            }
        }

        public void AddAnchor(int Idx)
        {
            foreach (Anchor anch in Anchors)
            {
                if (anch.Id == Idx)
                {
                    Debug.WriteLine("-------------");
                    Debug.WriteLine("bruh");
                    Debug.WriteLine("-------------");
                    return;
                }
            }
            Anchor an = new Anchor(Idx);
            Anchors.Add(an);
            XmlNode AnchorItem = _onAnchorAdd(an.Id);
            an.Init(AnchorItem, propinfo.SelectSingleNode(".//" + "aPropMetaData"), _onSetAnchor, _onAddAnchorPropertyTex, _onAddAnchorPropertyXML, _onRemoveAnchorPropertyXML, isLoadProcess);
        }

        public void RemoveAnchor(int AnchorID, string anchorType)
        {
            XmlNode aAnchors = propinfo.SelectSingleNode(".//" + "aAnchors");
            int id = 0;
            Dictionary<int, Dictionary<int, string[]>> props = new Dictionary<int, Dictionary<int, string[]>>();
            int id2 = 0;
            int id3 = 0;
            foreach (XmlNode item in aAnchors.SelectNodes("Item"))
            {
                XmlNode anchorName = item.SelectSingleNode("anchor");
                string propValues = item.SelectSingleNode("props").InnerText;
                Dictionary<int, string[]> correct = new Dictionary<int, string[]>();
                propValues = Regex.Replace(propValues, "  +", "", RegexOptions.Compiled);
                propValues = propValues.Replace("\n", "").Replace("\r", "");
                string[] anchorProps = propValues.Split(' ');
                for (int i = 0;i< anchorProps.Length;i++)
                {
                    if (anchorProps[i] == "" || anchorProps[i] == " ") continue;
                    if (anchorName.InnerText == anchorType)
                    {
                        correct.Add(i, new string[] { anchorProps[i], "true", id2.ToString(), anchorName.InnerText });
                    }
                    else
                    {
                        correct.Add(i, new string[] { anchorProps[i], "false", id2.ToString(), anchorName.InnerText });
                    }
                    id2++;
                }
                props.Add(id, correct);
                id++;
            }
            foreach (var item in props)
            {
                bool anchorRemoved = false;
                List<XmlNode> ad = new List<XmlNode>();
                foreach(var item2 in props[item.Key])
                {
                    if (item2.Value[1] == "true")
                    {
                        XmlNode anchorItem = propinfo.SelectSingleNode(".//" + "aAnchors");
                        XmlNode propMetaData = propinfo.SelectSingleNode(".//" + "aPropMetaData");
                        XmlNode metaRemove = propMetaData.ChildNodes[Convert.ToInt32(item2.Value[2])];
                        XmlNode aAnchorToBeRemoved = anchorItem.ChildNodes[item.Key];
                        if (metaRemove != null)
                            ad.Add(metaRemove);
                        if (aAnchorToBeRemoved != null && !anchorRemoved)
                            anchorItem.RemoveChild(aAnchorToBeRemoved);anchorRemoved = true;
                    }
                }
                for (int i = 0; i < ad.Count; i++)
                {
                    XmlNode elem = propinfo.SelectSingleNode(".//" + "aPropMetaData");
                    elem.RemoveChild(ad[i]);
                }
            }
            foreach (Anchor anch in Anchors)
            {
                if (anch.Id == AnchorID)
                {
                    Anchors.RemoveAt(id3);
                    break;
                }
                id3++;
            }
        }

        public int GetAnchorProps(int Idx)
        {
            for (int i = 0;i<Anchors.Count;i++)
            {
                if (Anchors[i].Id == Idx)
                {
                    Anchors.Remove(Anchors[i]);
                }
            }
            return -1;
        }


        public void AnchorPrfAlphaSwitched(int AnchorID, int PropID, Boolean Value)
        {
            foreach (Anchor anch in Anchors)
            {
                if (anch.Id == AnchorID)
                    anch.AnchorPrfAlphaSwitched(PropID, Value);
            }
        }

        public int AddAnchorProperty(int AnchorID)
        {
            foreach (Anchor anch in Anchors)
            {
                if (anch.Id == AnchorID)
                    return anch.AddProperty(null);
            }
            return -1;
        }


        public void RemoveAnchorPropertyByIdx(int AnchorID, int Idx)
        {
            foreach (Anchor anch in Anchors)
                if (anch.Id == AnchorID)
                    foreach (AnchorProperty ap in anch.Properties)
                        if (ap.ID == Idx)
                        {
                            anch.RemoveAnchorPropertyByIdx(AnchorID, Idx);

                            return;
                        }

        }
    }
}
