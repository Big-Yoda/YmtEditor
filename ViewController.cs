using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YMTEditor
{
    class ViewController
    {
        public Document doc;
        private OnDataLoaded _onDataLoaded { get; set; }
        private OnSetCompValue _onSetCompValue { get; set; }
        private OnSetAvailableDrawable _onSetAvailableDrawable { get; set; }
        private OnAddAnchor _onAddAnchor { get; set; }
        private OnAddAnchorProperty _onAddAnchorProperty { get; set; }
        private OnRefreshXML _onRefreshXML { get; set; } = null;

        private void OnDataLoad()
        {
            _onDataLoaded();
        }

        private void SetCompValue(int CompNumb, Boolean Included)
        {
            _onSetCompValue(CompNumb, Included);
        }
        private void SetAvailableDrawable(int CompNumb, int DrawableID, int NumTex, int Mask, int TexId)
        {
            _onSetAvailableDrawable(CompNumb, DrawableID, NumTex, Mask, TexId);
        }

        private void OnSetAnchor(int AnchorIdx, Anchor anchor)
        {
            _onAddAnchor(anchor.AnchorName);
            foreach (AnchorProperty prop in anchor.Properties) 
            {
                _onAddAnchorProperty(AnchorIdx, prop.NumTextures, prop.IsPrfAlpha);
            }
        }

        private void OnRefreshXMLProc(string XmlString) 
        {
            _onRefreshXML?.Invoke(XmlString);
        }

        public ViewController() 
        {
            doc = new Document();
            doc.Init(OnDataLoad, SetCompValue, SetAvailableDrawable, OnSetAnchor, OnRefreshXMLProc);
        }
        public void Init(OnDataLoaded onDataLoaded, OnSetCompValue onSetCompValue, OnSetAvailableDrawable onSetAvailableDrawable,
            OnAddAnchor onAddAnchor, OnAddAnchorProperty onAddAnchorProperty, OnRefreshXML onRefreshXML)
        {
            _onDataLoaded = onDataLoaded;
            _onSetCompValue = onSetCompValue;
            _onSetAvailableDrawable = onSetAvailableDrawable;
            _onAddAnchor = onAddAnchor;
            _onAddAnchorProperty = onAddAnchorProperty;
            _onRefreshXML = onRefreshXML;
        }

        public void OpenFile(string FilePath)
        {
            doc.Clear();
            doc.Load(FilePath);
        }

        public void OpenStream(Stream stream) 
        {
            doc.Clear();
            doc.LoadStream(stream);
        }
        public void SaveFile(string FilePath)
        {
            doc.Save(FilePath);
        }

        public void NumTextureChanged(int ComponentID, int DrawableID, int Value, int TexId) //+
        {
            doc.NumTextureChanged(ComponentID, DrawableID, Value, TexId);
        }

        public void MaskChanged(int ComponentID, int DrawableID, int Value) //+
        {
            doc.MaskChanged(ComponentID, DrawableID, Value);
        }

        public void TextIDChanged(int ComponentID, int DrawableID, string Value) //+
        {
            doc.TextIDChanged(ComponentID, DrawableID, Value);
        }

        public void DrawableAdded(int ComponentID) 
        {
            doc.DrawableAdded(ComponentID);
        }
        public void DrawableDeleted(int ComponentID) 
        {
            doc.DrawableDeleted(ComponentID);
        }
        public void ComponentSwitched(int ComponentID, Boolean Value) 
        {
            doc.ComponentSwitched(ComponentID, Value);
        }

        public void AnchorAdded(int AnchorID) 
        {
            doc.AnchorAdded(AnchorID);
        }

        public void AnchorRemove(int AnchorID, string anchorLabel)
        {
            doc.AnchorRemove(AnchorID, anchorLabel);
        }

        public void AnchorPropertyAdded(int AnchorID) 
        {
            doc.AnchorPropertyAdded(AnchorID);
        }

        public void AnchorPropertyRemoved(int AnchorID)
        {
            doc.AnchorPropertyRemoved(AnchorID);
        }

        public void RemoveAnchorProperty(int AnchorId, int propertyIdx) 
        {
            doc.RemoveAnchorProperty(AnchorId, propertyIdx);
        }

        public void AnchorTypeChanged(int AnchorID, string AnchorType) 
        {
            doc.AnchorTypeChanged(AnchorID, AnchorType);
        }
        public void AnchorPropNumTexturesChanged(int AnchorID, int PropID, int NumTextures) 
        {
            doc.AnchorPropNumTexturesChanged(AnchorID, PropID, NumTextures);
        }
        public void AnchorPrfAlphaSwitched(int AnchorID, int PropID, Boolean Value) 
        {
            doc.AnchorPrfAlphaSwitched(AnchorID, PropID, Value);
        }

        public XmlDocument getXmlDoc() 
        {
            return doc.getXmlDoc();
        }
    }
}
