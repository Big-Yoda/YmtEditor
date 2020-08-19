using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YMTEditor
{
    public class ComponentData
    {
        private XmlDocument doc { get; set; }
        private XmlNode availCompNode { get; set; }
        private XmlNode aComponentData3 { get; set; }
        private XmlNode compInfosNode { get; set; }
        private OnSetCompValue _onSetAvaliableComp { get; set; } = null;
        private OnSetAvailableDrawable _onSetAvailableDrawable { get; set; }

        public List<Component> Components;

        public ComponentData() 
        {
            Components = new List<Component>();
        }

        public void Init(XmlDocument Doc, XmlNode AvailComp, XmlNode ComponentData, XmlNode CompInfos, OnSetCompValue onSetAvaliableComp,
            OnSetAvailableDrawable onSetAvailableDrawable) 
        {
            doc = Doc;
            availCompNode = AvailComp;
            aComponentData3 = ComponentData;
            compInfosNode = CompInfos;
            _onSetAvaliableComp = onSetAvaliableComp;
            _onSetAvailableDrawable = onSetAvailableDrawable;

            ParseData();
        }

        private Component AddComponent(int CompNumb, int val, Boolean Included) 
        {
            Component res = new Component(CompNumb, val, Included);
            Components.Add(res);
            res.Init(_onSetAvailableDrawable, CreateDocElement);
            return res;
        }

        public void SetAvaliableComp(int CompNumb, int val, Boolean Included)
        {
            AddComponent(CompNumb, val, Included);
            if (_onSetAvaliableComp != null)
                _onSetAvaliableComp(CompNumb, Included);
        }

        public void Clear() 
        {
            Components.Clear();
        }

        public void ComponentSwitched(int ComponentID, Boolean Value)
        {
            int number = 0;
            int curr_nr = -1;
            Component currComponent = null;
            foreach (Component comp in Components)
            {
                if (comp.ID < ComponentID)
                {
                    if (comp.STATEID != 255)
                        number++;
                }
                else
                {
                    if (comp.ID == ComponentID)
                    {
                        comp.STATEID = Value ? number++ : 255;
                        curr_nr = number;
                        currComponent = comp;
                    }
                    else
                        comp.STATEID = comp.STATEID != 255 ? number++ : 255;
                }
            }
            string availComp = "";
            foreach (Component comp in Components)
            {
                if (availComp == "")
                    availComp = comp.STATEID.ToString();
                else
                    availComp = availComp + " " + comp.STATEID.ToString();
            }

            if (availCompNode != null)
                availCompNode.InnerText = availComp;
            if (Value)
            {
                Boolean added = false;
                int nr = -1;
                if (aComponentData3 != null)
                {
                    foreach (XmlNode node in aComponentData3.ChildNodes)
                        if ((node.Name == "Item") && (++nr == curr_nr - 1))
                        {
                            aComponentData3.InsertBefore(AddCompNode(currComponent), node);
                            added = true;
                            break;
                        }
                    if (!added)
                        aComponentData3.AppendChild(AddCompNode(currComponent));
                }

                added = false;
                foreach (XmlNode node in compInfosNode.ChildNodes)
                {
                    if (node.Name == "Item")
                    {
                        foreach (XmlNode prop in node.SelectNodes("hash_D12F579D"))
                            if ((Convert.ToInt32(prop.Attributes["value"].Value) > ComponentID) && (!added))
                            {
                                compInfosNode.InsertBefore(AddCompInfoNode(ComponentID), node);
                                added = true;
                                break;
                            }
                    }
                }
                if (!added)
                    compInfosNode.AppendChild(AddCompInfoNode(ComponentID));
            }
            else
            {
                int nr = -1;
                if (aComponentData3 != null)
                    foreach (XmlNode node in aComponentData3.ChildNodes)
                        if ((node.Name == "Item") && (++nr == curr_nr))
                        {
                            aComponentData3.RemoveChild(node);
                            break;
                        }

                List<XmlNode> forDel = new List<XmlNode>();
                foreach (XmlNode node in compInfosNode.ChildNodes)
                {
                    XmlNode prop = node.SelectSingleNode(".//" + "hash_D12F579D");
                    if ((prop != null) && (Convert.ToInt32(prop.Attributes["value"].Value) == ComponentID))
                    {
                        forDel.Add(node);
                    }
                }
                for(int i = forDel.Count - 1; i >= 0; i--)
                    compInfosNode.RemoveChild(forDel[i]);
            }
            if (currComponent.STATEID != 255)
                currComponent.OnEnable();
        }

        public void DrawableDeleted(int ComponentID)
        {
            Component selComp;
            XmlNode DrawblItem = CompItemNode(ComponentID, out selComp);
            if (DrawblItem == null)
                return;
            selComp.DeleteLastDrawable();
            //foreach (XmlNode drawbl in DrawblItem.SelectNodes("aDrawblData3"))
                //drawbl.RemoveChild(drawbl.ChildNodes[drawbl.ChildNodes.Count-1]);
            foreach (XmlNode AvailTex in DrawblItem.SelectNodes("numAvailTex"))
            {
                XmlElement elemtexId = (XmlElement)AvailTex;
                elemtexId.SetAttribute("value", selComp.AvailTex().ToString());
            }

            foreach (XmlNode node in compInfosNode.ChildNodes)
            {
                foreach (XmlNode prop in node.SelectNodes("hash_D12F579D"))
                {
                    foreach (XmlNode prop2 in node.SelectNodes("hash_FA1F27BF"))
                    {
                        if ((Convert.ToInt32(prop.Attributes["value"].Value) == ComponentID)
                            && (Convert.ToInt32(prop2.Attributes["value"].Value) == selComp.Drawables.Count))
                            compInfosNode.RemoveChild(node);
                    }
                }
            }
        }

        public void DrawableAdded(int ComponentID)
        {
            Component comp = ComponentByID(ComponentID);
            if (comp != null)
                comp.DrawableAdded();
        }

        public void TextIDChanged(int ComponentID, int DrawableID, string Value)
        {
            Component comp = ComponentByID(ComponentID);
            if (comp != null)
                comp.TextIDChanged(DrawableID, Value);
        }

        public void MaskChanged(int ComponentID, int DrawableID, int Value)
        {
            Component comp = ComponentByID(ComponentID);
            if (comp != null)
                comp.MaskChanged(DrawableID, Value);
        }

        public void NumTextureChanged(int ComponentID, int DrawableID, int Value, int TexId)
        {
            Component comp = ComponentByID(ComponentID);
            if (comp != null)
                comp.NumTextureChanged(DrawableID, Value, TexId);
        }

        private XmlElement CreateDocElement(string ElementName) 
        { 
            if(doc != null)
                return doc.CreateElement(ElementName);
            return null;
        }

        private XmlNode CompItemNode(int ComponentID, out Component Comp)
        {
            Comp = null;
            if ((ComponentID < 0) || (ComponentID > 11))
                return null;
            Comp = Components[ComponentID];
            if (aComponentData3 != null)
            {
                int StateID = Comp.STATEID;
                if (StateID == 255)
                    return null;
                int i = -1;
                foreach (XmlNode node in aComponentData3.ChildNodes)
                {
                    i++;
                    if (i == StateID)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        private Component ComponentByID(int ID)
        {
            foreach (Component comp in Components)
                if (comp.ID == ID)
                    return comp;
            return null;
        }

        private Component ComponentByStateID(int StateID) 
        {
            foreach (Component comp in Components)
                if (comp.STATEID == StateID)
                    return comp;
            return null;
        }

        private void ParseData() 
        {
            if (availCompNode != null)
                ParseAvailibleComp(availCompNode.InnerText);
            XmlNode CompItem;
            if (aComponentData3 != null && aComponentData3.HasChildNodes)
            {
                int stateId = 0;
                for (int i = 0; i < aComponentData3.ChildNodes.Count; i++)
                {
                    CompItem = aComponentData3.ChildNodes[i];
                    if (CompItem.Name == "Item")
                    {
                        Component comp = ComponentByStateID(stateId);
                        if (comp == null)
                            continue;
                        comp.FillData(CompItem, compInfosNode);
                        stateId++;
                    }
                }
            }
        }

        private void ParseAvailibleComp(string AvailibleComps)
        {
            int number = 0;
            string val = "";
            for (int i = 0; i < AvailibleComps.Length; i++)
            {
                if (AvailibleComps[i] == ' ')
                {
                    SetAvaliableComp(number, Convert.ToInt32(val), val != "255");
                    number++;
                    val = "";
                }
                else
                {
                    val += AvailibleComps[i];
                }
            }
            if (val != "")
                SetAvaliableComp(number, Convert.ToInt32(val), val != "255");
        }

        private XmlNode AddCompNode(Component component)
        {
            XmlElement elemRoot = doc.CreateElement("Item");
            XmlElement elemTex = doc.CreateElement("numAvailTex");
            elemTex.SetAttribute("value", "0");
            elemRoot.AppendChild(elemTex);
            XmlElement elemDrawblData = doc.CreateElement("aDrawblData3");
            elemRoot.AppendChild(elemDrawblData);
            elemDrawblData.SetAttribute("itemType", "CPVDrawblData");
            XmlElement elemDrawblItem = doc.CreateElement("Item");
            XmlElement elemPropMask = doc.CreateElement("propMask");
            elemDrawblItem.AppendChild(elemPropMask);
            elemPropMask.SetAttribute("value", "0");
            XmlElement elemnumAlternatives = doc.CreateElement("numAlternatives");
            elemDrawblItem.AppendChild(elemnumAlternatives);
            elemnumAlternatives.SetAttribute("value", "0");
            XmlElement elemTexData = doc.CreateElement("aTexData");
            elemDrawblItem.AppendChild(elemTexData);
            elemTexData.SetAttribute("itemType", "CPVTextureData");
            XmlElement elemclothData = doc.CreateElement("clothData");
            elemDrawblItem.AppendChild(elemclothData);
            XmlElement elemownsCloth = doc.CreateElement("ownsCloth");
            elemclothData.AppendChild(elemownsCloth);
            elemownsCloth.SetAttribute("value", "false");
            elemDrawblData.AppendChild(elemDrawblItem);
            component.FillData(elemRoot, compInfosNode);
            return elemRoot;
        }

        private XmlNode AddCompInfoNode(int CompID)
        {
            Component comp = ComponentByID(CompID);
            if (comp != null)
                return comp.AddCompInfoNode(CompID);
            return null;
        }
    }
}
