using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using YMTEditor;

namespace YMTEditor
{
    internal static class WindowExtensions
    {
        // from winuser.h
        private const int GWL_STYLE = -16,
                          WS_MAXIMIZEBOX = 0x10000,
                          WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        internal static void HideMinimizeAndMaximizeButtons(this Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }
    }

    public partial class MainWindow : Window
    {
        private bool LoadState { get; set; } = false;
        string EditedFile { get; set; } = "";
        ViewController controller;
        CheckBox[] CheckBoxes; 
        StackPanel[] Panels;
        TextBox[] PropMasks;
        TextBox[] NumTextures;
        ComboBox[] TextIds;
        int[] DrawableIdxs;
        XMLView viewer;// = null;
        private bool hasEdited { get; set; } = false;
        Update latest = new Update();

        public MainWindow()
        {
            latest.main(this);
            InitializeComponent();
            this.Title = $"Ped YMT Editor | Version {Properties.Resources.version}, {Properties.Resources.stage}{(latest.isLatest ? "" : " | OUTDATED VERSION")}";
            CheckBoxes = new CheckBox[] { chkHead, chkBerd, chkHair, chkArms, chkPants, chkHand, chkFeet, chkTeef, chkAccs, chkTask, chkDecl, chkJBIB };
            Panels = new StackPanel[] { spHead, spBerd, spHair, spArms, spPants, spHand, spFeet, spTeef, spAccs, spTask, spDecl, spJBIB };
            NumTextures = new TextBox[] { edHeadNumTextures, edBerdNumTextures, edHairNumTextures, edArmsNumTextures, edPantsNumTextures, edHandNumTextures,
                edFeetNumTextures, edTeefNumTextures, edAccsNumTextures, edTaskNumTextures, edDeclNumTextures, edJBIBNumTextures };
            PropMasks = new TextBox[] { edHeadPropMask, edBerdPropMask, edHairPropMask, edArmsPropMask, edPantsPropMask, edHandPropMask, edFeetPropMask, 
                edTeefPropMask, edAccsPropMask, edTaskPropMask, edDeclPropMask, edJBIBPropMask };
            TextIds = new ComboBox[] { cbxHeadTextId, cbxBerdTextId, cbxHairTextId, cbxArmsTextId, cbxPantsTextId, cbxHandTextId, cbxFeetTextId, cbxTeefTextId,
                cbxAccsTextId, cbxTaskTextId, cbxDeclTextId, cbxJBIBTextId };
            DrawableIdxs = new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            controller = new ViewController();
            controller.Init(SetEnabledForm, SetCompValue, SetAvailableDrawable, AddAnchor, addAnchorProp, OnRefreshXML);
            spMain.IsEnabled = false;
            fSave.IsEnabled = false;
            fSaveAs.IsEnabled = false;

            viewer = new XMLView();

            //viewer.Visibility = Visibility.Visible;
            this.SourceInitialized += (x, y) =>
            {
                this.HideMinimizeAndMaximizeButtons();
            };
           viewer.Load(controller.getXmlDoc());
           viewer.onApplyXMLStream = OnRefreshXMLStream;
        }

        private void OnRefreshXML(string XmlString) 
        {
            viewer.Load(controller.getXmlDoc());
        }

        private void OnRefreshXMLStream(Stream XmlStream) 
        {
            LoadState = true;
            ClearData();
            controller.OpenStream(XmlStream);
            LoadState = false;
        }

        private void SetAvailableDrawable(int pnl, int DrawableID, int NumTex, int Mask, int TexId) 
        {
            StackPanel panel = SelectedPanel(pnl);
            if (panel != null) 
            {
                TextBox tbNumTex; TextBox tbMask; ComboBox cbxTextId;
                if (DrawableID > 0)
                    AddDrawable(panel, DrawableID + 1, out tbNumTex, out tbMask, out cbxTextId);
                else 
                {
                    tbNumTex = NumTextures[pnl];
                    tbMask = PropMasks[pnl];
                    cbxTextId = TextIds[pnl];
                }
                tbNumTex.Text = NumTex.ToString();
                tbMask.Text = Mask.ToString();
                cbxTextId.Text = TexId.ToString();
            }
        }

        private void SetEnabledForm()
        {
            spMain.IsEnabled = true;
            fSave.IsEnabled = true;
            fSaveAs.IsEnabled = true;
        }

        private StackPanel SelectedPanel(int pnlID) 
        {
            if (pnlID > Panels.Length - 1)
                return null;
            else
                return Panels[pnlID];
        }

        private CheckBox SelectedCbx(int cbxId) 
        {
            if(cbxId > CheckBoxes.Length-1)
                return null;
            else
                return CheckBoxes[cbxId];
        }

        private void SetCompValue(int CompNumb, Boolean Included)
        {
            CheckBox selected = SelectedCbx(CompNumb);
            if(selected != null)
                selected.IsChecked = Included;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        void CheckRemoveDrawableButton(Button btn, bool idk) 
        {
            StackPanel pnl = ((btn.Parent as Grid).Parent as StackPanel);
            btn.IsEnabled = pnl.Children.Count > (idk ? 2 : 3);
        }

        void CheckRemoveAnchorPropertyButton(Button btn, bool idk)
        {
            StackPanel pnl = ((btn.Parent as Grid).Parent as StackPanel);
            btn.IsEnabled = pnl.Children.Count > (idk ? 2 : 3);
        }

        int RemoveDrawable(object sender) 
        {
            Grid grd = (Grid)((sender as Button).Parent);
            StackPanel sp1 = (StackPanel)grd.Parent;
            Border brd1 = (Border)sp1.Parent;
            StackPanel sp2 = (StackPanel)brd1.Parent;
            int idx = -1;
            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if (sp2.Children[i] == brd1)
                {
                    idx = i;
                    break;
                }
            }
            StackPanel pnl = (((sender as Button).Parent as Grid).Parent as StackPanel);
            pnl.Children.Remove(pnl.Children[pnl.Children.Count - 2]);
            DrawableIdxs[idx]--;
            CheckRemoveDrawableButton(sender as Button, false);
            return idx;
        }

        private int AddAnchor(string AnchorName) 
        {
            Border newBorder = new Border();
            newBorder.Background = GetColor(50);
            newBorder.BorderBrush = GetColor(146);
            newBorder.VerticalAlignment = VerticalAlignment.Top;
            //newBorder.Background = Brushes.GhostWhite;
            //newBorder.BorderBrush = Brushes.Gainsboro;
            newBorder.Margin = new Thickness(10, 10, 10, 0);
            newBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            newBorder.Height = Double.NaN;
            brdAnchors.Children.Add(newBorder);

            StackPanel sp = new StackPanel();
            sp.Height = Double.NaN;
            sp.VerticalAlignment = VerticalAlignment.Top;
            newBorder.Child = sp;

            StackPanel sp2 = new StackPanel();
            sp2.Orientation = Orientation.Horizontal;
            sp2.VerticalAlignment = VerticalAlignment.Center;
            sp2.Height = 30;
            sp2.Margin = new Thickness(0, 5, 0, 0);
            sp.Children.Add(sp2);

            Label lbl = new Label();
            lbl.FontSize = 14;
            lbl.Content = "Anchor:";
            lbl.Foreground = GetColor(146);
            sp2.Children.Add(lbl);

            ComboBox cbx = new ComboBox();
            cbx.Width = 145;
            cbx.Margin = new Thickness(0, 0, 5, 0);
            cbx.VerticalAlignment = VerticalAlignment.Top;
            cbx.Height = 26;
            cbx.ItemsSource = new List<string> { "HEAD", "EYES", "EARS", "MOUTH", "LEFT_HAND", "RIGHT_HAND", 
                "LEFT_WRIST", "RIGHT_WRIST", "HIP", "LEFT_FOOT", "RIGHT_FOOT", "UNK_604819740", "UNK_2358626934" };
            cbx.SelectionChanged += cbxAnchorId_SelectionChanged;
            sp2.Children.Add(cbx);

            Button btn3 = new Button();
            btn3.Width = 110;
            btn3.Margin = new Thickness(0, 0, 0, 0);
            btn3.VerticalAlignment = VerticalAlignment.Top;
            btn3.Height = 26;
            btn3.Content = "Delete Anchor";
            btn3.Background = GetColor(64);
            btn3.Foreground = GetColor(146);
            btn3.BorderBrush = GetColor(137);
            btn3.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn3.Click += AnchorPanelRemoveClick;
            sp2.Children.Add(btn3);

            Grid newGrid = new Grid();
            newGrid.Height = 40;
            newGrid.Width = Double.NaN;
            sp.Children.Add(newGrid);
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(48, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(71, GridUnitType.Star);
            newGrid.ColumnDefinitions.Add(c1);
            newGrid.ColumnDefinitions.Add(c2);

            Button btn = new Button();
            btn.Content = "Add prop";
            btn.HorizontalAlignment = HorizontalAlignment.Right;
            btn.Margin = new Thickness(0, 5, 114, 5);
            btn.Width = 90;
            btn.Background = GetColor(64);
            btn.Foreground = GetColor(146);
            btn.BorderBrush = GetColor(137);
            btn.Click += AddAnchorPropertyClick;
            Grid.SetColumn(btn, 1);
            newGrid.Children.Add(btn);

            Button btn2 = new Button();
            btn2.Content = "Remove";
            btn2.HorizontalAlignment = HorizontalAlignment.Right;
            btn2.Margin = new Thickness(0, 5, 10, 5);
            btn2.Width = 90;
            btn2.Background = GetColor(64);
            btn2.Foreground = GetColor(146);
            btn2.BorderBrush = GetColor(137);
            btn2.Click += RemoveAnchorPropertyClick;
            Grid.SetColumn(btn2, 1);
            newGrid.Children.Add(btn2);
            CheckRemoveAnchorPropertyButton(btn2, false);

            newBorder.Child = sp;
            return brdAnchors.Children.Count-1;
        }

        private int AnchorPanelIdx(StackPanel pnl) 
        {
            for (int i = 0; i < brdAnchors.Children.Count; i++) 
            {
                Border newBorder = (Border)brdAnchors.Children[i];
                if ((StackPanel)newBorder.Child == pnl)
                    return i;
            }
            return -1;
        }

        private void AddAnchorPropertyClick(object sender, RoutedEventArgs e)
        {
            TextBox Tbx; CheckBox Cbx;
            StackPanel pnl = (((sender as Button).Parent as Grid).Parent as StackPanel);
            int AnchorId = AnchorPanelIdx(pnl);
            AddAnchorProperty(AnchorId, out Tbx, out Cbx);
            controller.AnchorPropertyAdded(AnchorId);
            
            for (int a = 0; a < pnl.Children.Count; a++)
            {
                if (pnl.Children[a].GetType() == typeof(Grid))
                {
                    for (int b = 0; b < (pnl.Children[a] as Grid).Children.Count; b++)
                    {
                        if (((pnl.Children[a] as Grid).Children[b] as Button).Content.ToString() == "Remove")
                        {
                            CheckRemoveDrawableButton(((pnl.Children[a] as Grid).Children[b] as Button), true);
                            break;
                        }
                    }
                }
            }
        }
        
        private void AnchorPanelRemoveClick(object sender, RoutedEventArgs e)
        {
            if (LoadState)
                return;

            int AnchorID = -1;
            Button src = (Button)sender;
            StackPanel sp1 = (StackPanel)src.Parent;
            StackPanel sp2 = (StackPanel)sp1.Parent;
            Border brdr1 = (Border)sp2.Parent;
            StackPanel sp3 = (StackPanel)brdr1.Parent;
            string anchorType = $"ANCHOR_{(sp1.Children[1] as ComboBox).SelectedValue}";
            
            for (int i = 0; i < sp3.Children.Count; i++) {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr1)) {
                    AnchorID = i;
                    break;
                }
            }
            sp3.Children.RemoveAt(AnchorID);
            controller.AnchorRemove(AnchorID, anchorType);
        }

        private void RemoveAnchorPropertyClick(object sender, RoutedEventArgs e) 
        {
            StackPanel pnl = (((sender as Button).Parent as Grid).Parent as StackPanel);
            int AnchorId = AnchorPanelIdx(pnl);
            int idx = pnl.Children.Count - 2;
            pnl.Children.Remove(pnl.Children[idx]);
            controller.RemoveAnchorProperty(AnchorId, idx-1);
            CheckRemoveAnchorPropertyButton(sender as Button, false);
        }

        private StackPanel getAnchorParentByID(int AnchorId) 
        {
            if (AnchorId <= brdAnchors.Children.Count-1 )
            {
                Border brd = (Border)brdAnchors.Children[AnchorId];
                StackPanel anch = (StackPanel)brd.Child;
                if (anch != null)
                    return anch; 
            }
            return null;
        }

        private void addAnchorProp(int AnchorIdx, int NumTextures, bool IsPrfAlpha)
        {
            TextBox Tbx; CheckBox Cbx;
            AddAnchorProperty(AnchorIdx, out Tbx, out Cbx);
            Tbx.Text = NumTextures.ToString();
            Cbx.IsChecked = IsPrfAlpha;
            
        }

        private Brush GetColor(int r, int g, int b, int alpha)
        {
            return new SolidColorBrush(Color.FromArgb((byte)alpha, (byte)r, (byte)g, (byte)b));
        }

        private Brush GetColor(int rgb, int alpha = 255)
        {
            return GetColor(rgb, rgb, rgb, alpha);
        }
        
        private void AddAnchorProperty(int AnchorId, out TextBox Tbx, out CheckBox Cbx)
        {
            Border newBorder = new Border();
            newBorder.VerticalAlignment = VerticalAlignment.Top;
            newBorder.BorderThickness = new Thickness(1, 1, 1, 1);
            newBorder.Background = GetColor(55);
            newBorder.BorderBrush = Brushes.Gainsboro;
            newBorder.Margin = new Thickness(10, 0, 10, 0);
            newBorder.Height = 55;

            Grid newGrid = new Grid();
            newGrid.Width = Double.NaN;
            newGrid.Margin = new Thickness(0, 0, 0, 0);
            newBorder.Child = newGrid;
            newBorder.BorderBrush = GetColor(146);
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(113, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(114, GridUnitType.Star);
            newGrid.ColumnDefinitions.Add(c1);
            newGrid.ColumnDefinitions.Add(c2);

            StackPanel sp = new StackPanel();
            Grid.SetColumn(sp, 0);
            sp.Orientation = Orientation.Horizontal;
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.Height = 30;
            sp.Margin = new Thickness(0, 2, 0, 0);
            newGrid.Children.Add(sp);

            Label lbl = new Label();
            lbl.Content = "Num. textures";
            lbl.VerticalContentAlignment = VerticalAlignment.Center;
            lbl.Foreground = GetColor(146);
            sp.Children.Add(lbl);

            TextBox tbx = new TextBox();
            tbx.HorizontalAlignment = HorizontalAlignment.Left;
            tbx.Height = 26;
            tbx.TextWrapping = TextWrapping.Wrap;
            tbx.Text = "1";
            tbx.Background = GetColor(146);
            tbx.BorderBrush = GetColor(100);
            sp.VerticalAlignment = VerticalAlignment.Center;
            tbx.Width = 89;
            tbx.PreviewTextInput += NumberValidationTextBox;
            tbx.TextChanged += edAnchorNumTex_TextChanged;
            sp.Children.Add(tbx);

            StackPanel sp2 = new StackPanel();
            Grid.SetColumn(sp2, 1);
            sp2.Orientation = Orientation.Horizontal;
            sp2.VerticalAlignment = VerticalAlignment.Center;
            sp2.HorizontalAlignment = HorizontalAlignment.Right;
            sp2.Height = 20;
            sp2.Width = 108;
            sp2.Margin = new Thickness(0, 15, 0, 0);
            newGrid.Children.Add(sp2);

            CheckBox cbx = new CheckBox();
            cbx.Content = "PRF__ALPHA";
            cbx.Foreground = GetColor(146);
            cbx.Click += chkRenderFlag_Click;
            sp2.HorizontalAlignment = HorizontalAlignment.Right;
            sp2.Height = 20;
            sp2.Width = 108;
            cbx.Background = GetColor(146);
            sp2.VerticalAlignment = VerticalAlignment.Center;
            sp2.Children.Add(cbx);

            newBorder.Child = newGrid;
            StackPanel Parent = getAnchorParentByID(AnchorId);
            if (Parent != null)
            {
                Parent.Children.Insert(Parent.Children.Count - 1, newBorder);
            }
            Tbx = tbx;
            Cbx = cbx;
            
        }

        void AddDrawable(StackPanel MainPanel, int DrawableIndex, out TextBox tbNumTex, out TextBox tbMask, out ComboBox cbxTextId) 
        {
            
            MainPanel.Background = GetColor(55);

            Border newBorder = new Border();
            newBorder.VerticalAlignment = VerticalAlignment.Top;
            newBorder.Margin = new Thickness(10, 0, 10, 0);
            newBorder.Background = GetColor(50);
            newBorder.BorderBrush = GetColor(146);
            newBorder.BorderThickness = new Thickness(1);

            Grid newGrid = new Grid();
            newGrid.Height = 70;
            newGrid.Width = Double.NaN;
            newGrid.Background = GetColor(50);

            newGrid.RowDefinitions.Add(new RowDefinition());
            newGrid.RowDefinitions.Add(new RowDefinition());
            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(184, GridUnitType.Star);
            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(103, GridUnitType.Star);
            ColumnDefinition c3 = new ColumnDefinition();
            c3.Width = new GridLength(166, GridUnitType.Star);
            newGrid.ColumnDefinitions.Add(c1);
            newGrid.ColumnDefinitions.Add(c2);
            newGrid.ColumnDefinitions.Add(c3);

            Label lbl = new Label();
            Grid.SetRow(lbl, 0);
            Grid.SetColumn(lbl, 0);
            lbl.FontSize = 14;
            lbl.Content = "Drawable" + DrawableIndex.ToString();
            lbl.Foreground = GetColor(200);
            newGrid.Children.Add(lbl);

            StackPanel sp = new StackPanel();
            Grid.SetRow(sp, 1);
            Grid.SetColumn(sp, 0);
            sp.Orientation = Orientation.Horizontal;
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.Width = Double.NaN;
            sp.Height = 30;
            sp.Margin = new Thickness(0, 3, 0, 2);
            newGrid.Children.Add(sp);

            Label lbl1 = new Label();
            lbl1.Content = "Num. textures";
            lbl1.Foreground = GetColor(146);
            sp.Children.Add(lbl1);

            TextBox tbx = new TextBox();
            tbx.HorizontalAlignment = HorizontalAlignment.Left;
            tbx.VerticalAlignment = VerticalAlignment.Top;
            tbx.Height = 26;
            tbx.Width = 60;
            tbx.TextWrapping = TextWrapping.Wrap;
            tbx.Text = "1";
            tbx.BorderBrush = GetColor(100);
            tbx.Background = GetColor(146);
            tbx.PreviewTextInput += NumberValidationTextBox;
            tbx.TextChanged += edNumTextures_TextChanged;
            tbNumTex = tbx;
            sp.Children.Add(tbx);

            StackPanel sp2 = new StackPanel();
            Grid.SetRow(sp2, 1);
            Grid.SetColumn(sp2, 1);
            sp2.Orientation = Orientation.Horizontal;
            sp2.VerticalAlignment = VerticalAlignment.Center;
            sp2.Height = 30;
            sp2.HorizontalAlignment = HorizontalAlignment.Right;
            sp2.Margin = new Thickness(0, 3, 0, 2);
            newGrid.Children.Add(sp2);

            Label lbl2 = new Label();
            lbl2.Content = "Mask";
            lbl2.Foreground = GetColor(146);
            sp2.Children.Add(lbl2);

            TextBox tbx2 = new TextBox();
            tbx2.HorizontalAlignment = HorizontalAlignment.Left;
            tbx2.VerticalAlignment = VerticalAlignment.Top;
            tbx2.Height = 26;
            tbx2.Width = 60;
            tbx2.TextWrapping = TextWrapping.Wrap;
            tbx2.Text = "1";
            tbx2.BorderBrush = GetColor(100);
            tbx2.Background = GetColor(146);
            tbx2.PreviewTextInput += NumberValidationTextBox;
            tbx2.TextChanged += edMask_TextChanged;
            tbMask = tbx2;
            sp2.Children.Add(tbx2);

            StackPanel sp3 = new StackPanel();
            Grid.SetRow(sp3, 1);
            Grid.SetColumn(sp3, 2);
            sp3.Orientation = Orientation.Horizontal;
            sp3.VerticalAlignment = VerticalAlignment.Center;
            sp3.Height = 30;
            sp3.HorizontalAlignment = HorizontalAlignment.Right;
            sp3.Margin = new Thickness(32, 3, 0, 2);
            newGrid.Children.Add(sp3);

            Label lbl3 = new Label();
            lbl3.Width = 41;
            lbl3.Content = "TexId";
            lbl3.Foreground = GetColor(146);
            sp3.Children.Add(lbl3);

            ComboBox cbx = new ComboBox();
            cbx.Width = 87;
            cbx.Margin = new Thickness(0, 0, 5, 0);
            cbx.VerticalAlignment = VerticalAlignment.Top;
            cbx.Height = 26;
            cbx.ItemsSource = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" };
            cbx.SelectionChanged += cbxHeadTextId_SelectionChanged;
            cbxTextId = cbx;
            sp3.Children.Add(cbx);

            newBorder.Child = newGrid;
            MainPanel.Children.Insert(MainPanel.Children.Count - 1, newBorder);
            
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Grid grd = (Grid)((sender as Button).Parent);
            StackPanel sp1 = (StackPanel)grd.Parent;
            Border brd1 = (Border)sp1.Parent;
            StackPanel sp2 = (StackPanel)brd1.Parent;
            int idx = -1;
            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if (sp2.Children[i] == brd1)
                {
                    idx = i;
                    break;
                }
            }
            for (int a = 0; a < grd.Children.Count; a++)
            {
                if ((grd.Children[a] as Button).Content.ToString() == "Remove") {
                    CheckRemoveDrawableButton(grd.Children[a] as Button, true);
                    break;
                }
            }
            TextBox tbNumTex; TextBox tbMask; ComboBox cbxTextId;
            AddDrawable(sp1, DrawableIdxs[idx]++, out tbNumTex, out tbMask, out cbxTextId);
            controller.DrawableAdded(idx);
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            controller.AnchorAdded(AddAnchor(""));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            controller.DrawableDeleted(RemoveDrawable(sender));
        }

        private void ClearData() 
        {
            chkHead.IsChecked = false;
            for (int i = spHead.Children.Count - 2; i > 1; i--)
                spHead.Children.RemoveAt(i);
            chkBerd.IsChecked = false;
            for (int i = spBerd.Children.Count - 2; i > 1; i--)
                spBerd.Children.RemoveAt(i);
            chkHair.IsChecked = false;
            for (int i = spHair.Children.Count - 2; i > 1; i--)
                spHair.Children.RemoveAt(i);
            chkArms.IsChecked = false;
            for (int i = spArms.Children.Count - 2; i > 1; i--)
                spArms.Children.RemoveAt(i);
            chkPants.IsChecked = false;
            for (int i = spPants.Children.Count - 2; i > 1; i--)
                spPants.Children.RemoveAt(i);
            chkHand.IsChecked = false;
            for (int i = spHand.Children.Count - 2; i > 1; i--)
                spHand.Children.RemoveAt(i);
            chkFeet.IsChecked = false;
            for (int i = spFeet.Children.Count - 2; i > 1; i--)
                spFeet.Children.RemoveAt(i);
            chkTeef.IsChecked = false;
            for (int i = spTeef.Children.Count - 2; i > 1; i--)
                spTeef.Children.RemoveAt(i);
            chkAccs.IsChecked = false;
            for (int i = spAccs.Children.Count - 2; i > 1; i--)
                spAccs.Children.RemoveAt(i);
            chkTask.IsChecked = false;
            for (int i = spTask.Children.Count - 2; i > 1; i--)
                spTask.Children.RemoveAt(i);
            chkDecl.IsChecked = false;
            for (int i = spDecl.Children.Count - 2; i > 1; i--)
                spDecl.Children.RemoveAt(i);
            chkJBIB.IsChecked = false;
            for (int i = spJBIB.Children.Count - 2; i > 1; i--)
                spJBIB.Children.RemoveAt(i);

            brdAnchors.Children.Clear();
        }

        private bool XmlFileValidation(string fileName)
        {
            XmlTextReader validatorReader = new XmlTextReader(fileName);
            validatorReader.Read();
            XmlDocument validatorDoc = new XmlDocument();
            validatorDoc.Load(validatorReader);
            XmlNode validatorRoot = validatorDoc.SelectSingleNode(".//" + "CPedVariationInfo");
            if (validatorRoot == null)
            {
                LoadState = false;
                MessageBox.Show("An error occured loading this file, Are you sure this is file was correctly exported?", "Error");
                return false;
            }
            else
            {
                XmlNode xNode2 = validatorDoc.SelectSingleNode(".//" + "availComp");
                if ((xNode2.InnerText.IndexOf(" ") == -1) || (xNode2.InnerText.IndexOf("FF") != -1))
                {
                    LoadState = false;
                    MessageBox.Show("An error occured loading this file, Are you sure this is file was correctly exported?", "Error");
                    return false;
                }
            }
            return true;
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                LoadState = true;
                if (!XmlFileValidation(openFileDialog.FileName))
                    return;
                ClearData();
                controller.OpenFile(openFileDialog.FileName);
                EditedFile = openFileDialog.FileName;
                LoadState = false;
            }
        }
        
        private void MenuOpenYMT_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Ped YMT Files (*.ymt)|*.ymt";
            if (openFileDialog.ShowDialog() == true)
            {
                LoadState = true;
                if (!XmlFileValidation(openFileDialog.FileName))
                    return;
                ClearData();
                controller.OpenFile(openFileDialog.FileName);
                EditedFile = openFileDialog.FileName;
                LoadState = false;
            }
#else
            MessageBox.Show("This feature is yet to be implemented");
#endif
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            LoadState = true;
            if (!File.Exists("template.ymt.xml"))
            {
                MessageBox.Show("An error occured creating a new Meta File, Are you sure the template file exists in the correct path? (Same location as the .exe File, Name MUST be 'template.ymt.xml'.)", "Error");
                LoadState = false;
                return;
            }
            if (!XmlFileValidation("template.ymt.xml"))
                return;
            ClearData();
            controller.OpenFile("template.ymt.xml");
            EditedFile = "template.ymt.xml";
            LoadState = false;
            
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            if (EditedFile == "template.ymt.xml")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                {
                    if (saveFileDialog.FileName == "template.ymt.xml")
                        saveFileDialog.FileName = "template2.ymt.xml";
                    controller.SaveFile(saveFileDialog.FileName);
                    EditedFile = saveFileDialog.FileName;
                }
            } else
            {
                controller.SaveFile(EditedFile);
            }
            //viewer.hasEdited = false;
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                controller.SaveFile(saveFileDialog.FileName);
                //viewer.hasEdited = false;
            }
        }

        /*private void MenuExit_Click(object sender, CancelEventArgs e)
        {
            //Window_Closed(sender, e);
        }*/

        private void edNumTextures_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoadState)
                return;

            int ComponentID = -1;
            int DrawableID = -1;
            TextBox src = (TextBox)sender;
            int temp = 0;
            bool canConvert = int.TryParse(src.Text, out temp);
            if (!canConvert)
                src.Text = "1";
            StackPanel sp1 = (StackPanel)src.Parent;
            Grid gr1 = (Grid)sp1.Parent;
            int TexId = -1;
            for (int i = 0; i < gr1.Children.Count; i++)
            {
                if (gr1.Children[i] is StackPanel) 
                {
                    StackPanel spCbx = (gr1.Children[i] as StackPanel);
                    for (int j = 0; j < spCbx.Children.Count; j++)
                    {
                        if ((spCbx.Children[j] is ComboBox))
                        {
                            TexId = (spCbx.Children[j] as ComboBox).Text!=""?Convert.ToInt32((spCbx.Children[j] as ComboBox).Text):0;
                            break;
                        }
                    }
                }
            }

            Border brdr1 = (Border)gr1.Parent;//dr
            StackPanel sp2 = (StackPanel)brdr1.Parent;//dr
            Border brdr2 = (Border)sp2.Parent;//comp
            StackPanel sp3 = (StackPanel)brdr2.Parent;//comp
            for (int i = 0; i < sp3.Children.Count; i++)
            {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr2))
                {
                    ComponentID = i;
                    break;
                }
            }
            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if ((sp2.Children[i] is Border) && ((Border)sp2.Children[i] == brdr1))
                {
                    DrawableID = i-1;
                    break;
                }
            }
            controller.NumTextureChanged(ComponentID, DrawableID, Convert.ToInt32(src.Text), TexId);
        }

        private void edMask_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoadState)
                return;

            int ComponentID = -1;
            int DrawableID = -1;
            TextBox src = (TextBox)sender;
            int temp = 0;
            bool canConvert = int.TryParse(src.Text, out temp);
            if (!canConvert)
                src.Text = "1";
            StackPanel sp1 = (StackPanel)src.Parent;
            Grid gr1 = (Grid)sp1.Parent;
            Border brdr1 = (Border)gr1.Parent;//dr
            StackPanel sp2 = (StackPanel)brdr1.Parent;//dr
            Border brdr2 = (Border)sp2.Parent;//comp
            StackPanel sp3 = (StackPanel)brdr2.Parent;//comp
            for (int i = 0; i < sp3.Children.Count; i++)
            {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr2))
                {
                    ComponentID = i;
                    break;
                }
            }
            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if ((sp2.Children[i] is Border) && ((Border)sp2.Children[i] == brdr1))
                {
                    DrawableID = i - 1;
                    break;
                }
            }
            controller.MaskChanged(ComponentID, DrawableID, Convert.ToInt32(src.Text));
        }

        private void cbxHeadTextId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadState)
                return;

            int ComponentID = -1;
            int DrawableID = -1;
            ComboBox src = (ComboBox)sender;
            StackPanel sp1 = (StackPanel)src.Parent;
            Grid gr1 = (Grid)sp1.Parent;
            Border brdr1 = (Border)gr1.Parent;//dr
            StackPanel sp2 = (StackPanel)brdr1.Parent;//dr
            Border brdr2 = (Border)sp2.Parent;//comp
            StackPanel sp3 = (StackPanel)brdr2.Parent;//comp
            for (int i = 0; i < sp3.Children.Count; i++)
            {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr2))
                {
                    ComponentID = i;
                    break;
                }
            }
            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if ((sp2.Children[i] is Border) && ((Border)sp2.Children[i] == brdr1))
                {
                    DrawableID = i - 1;
                    break;
                }
            }
            controller.TextIDChanged(ComponentID, DrawableID, src.SelectedItem as string);
        }

        private void chkHead_Click(object sender, RoutedEventArgs e)
        {
            if (LoadState)
                return;

            CheckBox chk = (CheckBox)sender;
            StackPanel sp1 = (StackPanel)chk.Parent;
            StackPanel sp2 = (StackPanel)sp1.Parent;
            for (int i = 0; i < sp2.Children.Count; i++) 
            {
                if (sp2.Children[i] == sp1) 
                {
                    controller.ComponentSwitched(i, chk.IsChecked == true);
                    break;
                }
            }
        }

        private void cbxAnchorId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadState)
                return;

            int AnchorID = -1;
            ComboBox src = (ComboBox)sender;
            StackPanel sp1 = (StackPanel)src.Parent;
            StackPanel sp2 = (StackPanel)sp1.Parent;
            Border brdr1 = (Border)sp2.Parent;
            StackPanel sp3 = (StackPanel)brdr1.Parent;

            for (int i = 0; i < sp3.Children.Count; i++)
            {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr1))
                {
                    AnchorID = i;
                    break;
                }
            }
            controller.AnchorTypeChanged(AnchorID, src.SelectedItem as string);
        }

        private void edAnchorNumTex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoadState)
                return;

            int AnchorID = -1;
            int PropID = -1;
            int NumTextures = -1;
            TextBox src = (TextBox)sender;
            int temp = 0;
            bool canConvert = int.TryParse(src.Text, out temp);
            if (!canConvert)
                src.Text = "1";
            NumTextures = Convert.ToInt32(src.Text);
            StackPanel sp1 = (StackPanel)src.Parent;
            Grid gr1 = (Grid)sp1.Parent;
            Border brdr1 = (Border)gr1.Parent;
            StackPanel sp2 = (StackPanel)brdr1.Parent;

            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if ((sp2.Children[i] is Border) && ((Border)sp2.Children[i] == brdr1))
                {
                    PropID = i-1;
                    break;
                }
            }

            Border brdr2 = (Border)sp2.Parent;
            StackPanel sp3 = (StackPanel)brdr2.Parent;
            for (int i = 0; i < sp3.Children.Count; i++)
            {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr2))
                {
                    AnchorID = i;
                    break;
                }
            }
            controller.AnchorPropNumTexturesChanged(AnchorID, PropID, NumTextures);
        }

        private void chkRenderFlag_Click(object sender, RoutedEventArgs e)
        {
            if (LoadState)
                return;

            int AnchorID = -1;
            int PropID = -1;
            CheckBox chk = (CheckBox)sender;
            Boolean res = chk.IsChecked == true;
            StackPanel sp1 = (StackPanel)chk.Parent;
            Grid gr1 = (Grid)sp1.Parent;
            Border brdr1 = (Border)gr1.Parent;
            StackPanel sp2 = (StackPanel)brdr1.Parent;

            for (int i = 0; i < sp2.Children.Count; i++)
            {
                if ((sp2.Children[i] is Border) && ((Border)sp2.Children[i] == brdr1))
                {
                    PropID = i-1;
                    break;
                }
            }

            Border brdr2 = (Border)sp2.Parent;
            StackPanel sp3 = (StackPanel)brdr2.Parent;
            for (int i = 0; i < sp3.Children.Count; i++)
            {
                if ((sp3.Children[i] is Border) && ((Border)sp3.Children[i] == brdr2))
                {
                    AnchorID = i;
                    break;
                }
            }

            controller.AnchorPrfAlphaSwitched(AnchorID, PropID, res);
        }

        private void setEdited(bool _hasEdited)
        {
            hasEdited = _hasEdited;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
        }

        private void XmlViewer_Click(object sender, EventArgs e)
        {
            viewer.Show();
        }

        private void Window_Closed(object sender, CancelEventArgs e)
        {
            
                //e.Cancel = true;
                //MessageBox.Show("You have unsaved changes, Would you like to save?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
            viewer.onApplyXMLStream = null;
            System.Windows.Application.Current.Shutdown();
        }
    }
}
