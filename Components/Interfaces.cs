using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YMTEditor
{
    public delegate void OnDataLoaded();
    public delegate void OnSetCompValue(int CompNumb, Boolean Included);
    public delegate void OnSetAvailableDrawable(int CompNumb, int DrawableID, int NumTex, int Mask, int TexId);
    public delegate void OnSetAnchor(int AnchorIdx, Anchor anchor);
    public delegate int OnAddAnchor(string AnchorName);
    public delegate void OnAddAnchorProperty(int AnchorIdx, int NumTextures, bool IsPrfAlpha);
    public delegate void OnAddAnchorPropertyTex(int TexId, XmlNode texData);
    public delegate XmlNode OnAddAnchorPropertyXML(int AnchorId, int PropertyId);
    public delegate void OnRemoveAnchorPropertyXML(int AnchorId, int PropertyId);
    public delegate XmlNode AnchorAddXMLEvnt(int AnchorID);
    public delegate Boolean IsLoadProcessDelegate();
    public delegate void OnNumTexturesChangedDelegate();
    public delegate XmlElement OnCreateDocElement(string ElementName);
    public delegate void OnRefreshXML(string XmlString);
    public delegate void OnRefreshXMLStream(Stream XmlStream);

}
