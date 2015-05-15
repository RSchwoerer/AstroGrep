using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace AstroGrep.Windows
{
   /// <summary>
   /// Used to retrieve language specific text for controls and generic messages.
   /// </summary>
   /// <remarks>
   /// AstroGrep File Searching Utility. Written by Theodore L. Ward
   /// Copyright (C) 2002 AstroComma Incorporated.
   /// 
   /// This program is free software; you can redistribute it and/or
   /// modify it under the terms of the GNU General Public License
   /// as published by the Free Software Foundation; either version 2
   /// of the License, or (at your option) any later version.
   /// 
   /// This program is distributed in the hope that it will be useful,
   /// but WITHOUT ANY WARRANTY; without even the implied warranty of
   /// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   /// GNU General Public License for more details.
   /// 
   /// You should have received a copy of the GNU General Public License
   /// along with this program; if not, write to the Free Software
   /// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
   /// 
   /// The author may be contacted at:
   /// ted@astrocomma.com or curtismbeard@gmail.com
   /// </remarks>
   /// <history>
   /// [Curtis_Beard]      07/31/2006	Created
   /// </history>
   public class Language
   {
      #region Declarations
      private static XmlDocument __XmlDoc = null;
      private static XmlNode __RootNode = null;
      private static XmlNode __XmlGenericNode = null;
      #endregion

      /// <summary>
      /// Initializes an instance of the Language class.
      /// </summary>
      private Language()
      {}

      #region Public Methods
      /// <summary>
      /// Loads the given language's file.
      /// </summary>
      /// <param name="language">String containing language</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// [Curtis_Beard]		10/11/2006	CHG: Close stream
      /// </history>
      public static void Load(string language)
      {
         try
         {
            System.Reflection.Assembly _assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string _name = _assembly.GetName().Name;

            Stream _stream = _assembly.GetManifestResourceStream(String.Format("{0}.Language.{1}.xml", _name, language));

            if (_stream != null)
            {
               string _contents = string.Empty;

               using (StreamReader _reader = new StreamReader(_stream))
               {
                  _contents = _reader.ReadToEnd();
               }

               _stream.Close();

               if (!_contents.Equals(string.Empty))
               {
                  __XmlDoc = new XmlDocument();
                  __XmlDoc.LoadXml(_contents);

                  __RootNode = __XmlDoc.SelectSingleNode("language");

                  if (__RootNode != null)
                     __XmlGenericNode = __RootNode.SelectSingleNode("generic");
               }
            }

            _assembly = null;
         }
         catch {}
      }

      /// <summary>
      /// Loads the given ComboBox with the available languages.
      /// </summary>
      /// <param name="combo">ComboBox to load</param>
      /// <history>
      /// [Curtis_Beard]		05/18/2007	Created
      /// </history>
      public static void LoadComboBox(ComboBox combo)
      {
         combo.Items.Clear();
         combo.DisplayMember = "DisplayName";
         combo.ValueMember = "Culture";

         if (Directory.Exists(LanguageLocation))
         {
            string[] files = Directory.GetFiles(LanguageLocation, "*.xml");

            if (files.Length > 0)
            {
               foreach (string file in files)
               {
                  // retrieve language information from file
                  string displayName = string.Empty;
                  string culture = string.Empty;
                  GetLanguageInformation(file, ref displayName, ref culture);

                  // valid language file
                  if (!displayName.Equals(string.Empty) && !culture.Equals(string.Empty))
                  {
                     LanguageItem item = new LanguageItem(displayName, culture);
                     combo.Items.Add(item);
                  }
               }
            }
         }

         // only load embedded language if a failure occurred
         if (combo.Items.Count == 0)
         {
            LanguageItem item = new LanguageItem("English (U.S.)", "en-us");
            combo.Items.Add(item);
         }
      }

		/// <summary>
		/// Loads the given ComboBox with the available languages.
		/// </summary>
		/// <param name="combo">ComboBox to load</param>
		/// <remarks>Legacy version until language files are external</remarks>
		/// <history>
		/// [Curtis_Beard]		10/11/2007	Created
		/// </history>
		public static void LegacyLoadComboBox(ComboBox combo)
		{
			combo.Items.Clear();
			combo.DisplayMember = "DisplayName";
			combo.ValueMember = "Culture";

			// legacy has a defined set of languages
			LanguageItem item = new LanguageItem("English", "en-us");
			combo.Items.Add(item);

			item = new LanguageItem("Espa�ol", "es-mx");
			combo.Items.Add(item);

			item = new LanguageItem("Deutsch", "de-de");
			combo.Items.Add(item);

			item = new LanguageItem("Italiano", "it-it");
			combo.Items.Add(item);

			item = new LanguageItem("Danish", "da-dk");
			combo.Items.Add(item);
		}

      /// <summary>
      /// Gets the location of all the language files.
      /// </summary>
      /// <history>
      /// [Curtis_Beard]		05/22/2007	Created
      /// </history>
      public static string LanguageLocation
      {
         get 
         {
            return Path.Combine(Constants.ProductLocation, "Language");
         }
      }

      /// <summary>
      /// Sets the given control's text property.
      /// </summary>
      /// <param name="control">Control to set</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static void SetControlText(Control control)
      {
         SetControlText(control, null);
      }

      /// <summary>
      /// Sets the given control's text property.
      /// </summary>
      /// <param name="control">Control to set</param>
      /// <param name="tip">ToolTip control</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static void SetControlText(Control control, ToolTip tip)
      {
         if (__RootNode != null)
         {
            string formName = control.FindForm().Name;
            XmlNode node = __RootNode.SelectSingleNode("screen[@name='" + formName + "']");
            XmlNode controlNode;

            if (node != null)
            {
               //node found, find control
               controlNode = node.SelectSingleNode("control[@name='" + control.Name + "']");

               if (controlNode != null)
               {
                  //found control node

                  //text
                  if (controlNode.Attributes["value"] != null)
                     control.Text = controlNode.Attributes["value"].Value;

                  //tooltip
                  if (tip != null && controlNode.Attributes["tooltip"] != null)
                     tip.SetToolTip(control, controlNode.Attributes["tooltip"].Value);
               }
            }
         }
      }

      /// <summary>
      /// Sets the given control's text and tooltip.
      /// </summary>
      /// <param name="control">ToolStripItem to set text/tooltip for.</param>
      /// <param name="tip">ToolTip object</param>
      /// <history>
      /// [Curtis_Beard]		09/27/2012	Initial: 1741735, support ToolStripItem
      /// </history>
      public static void SetToolStripItemText(ToolStripItem control, ToolTip tip)
      {
         if (__RootNode != null)
         {
            string formName = control.Owner.FindForm().Name;
            XmlNode node = __RootNode.SelectSingleNode("screen[@name='" + formName + "']");
            XmlNode controlNode;

            if (node != null)
            {
               //node found, find control
               controlNode = node.SelectSingleNode("control[@name='" + control.Name + "']");

               if (controlNode != null)
               {
                  //found control node

                  //text
                  if (controlNode.Attributes["value"] != null)
                     control.Text = controlNode.Attributes["value"].Value;

                  //tooltip
                  if (tip != null && controlNode.Attributes["tooltip"] != null)
                  {
                     control.ToolTipText = controlNode.Attributes["tooltip"].Value;
                  }
               }
            }
         }
      }

      /// <summary>
      /// Retrieve the control's text value in the loaded language file.
      /// </summary>
      /// <param name="control">Control to set</param>
      /// <returns>value of control's text in language file</returns>
      /// <history>
      /// [Curtis_Beard]		11/02/2006	Created
      /// </history>
      public static string GetControlText(Control control)
      {
         if (__RootNode != null)
         {
            string formName = control.FindForm().Name;
            XmlNode node = __RootNode.SelectSingleNode("screen[@name='" + formName + "']");
            XmlNode controlNode;

            if (node != null)
            {
               //node found, find control
               controlNode = node.SelectSingleNode("control[@name='" + control.Name + "']");

               if (controlNode != null)
               {
                  //text
                  if (controlNode.Attributes["value"] != null)
                     return controlNode.Attributes["value"].Value;
               }
            }
         }

         return string.Empty;
      }

      /// <summary>
      /// Sets the given context menuitem's text property.
      /// </summary>
      /// <param name="holder">Control containing ContextMenu</param>
      /// <param name="item">MenuItem to set</param>
      /// <history>
      /// [Curtis_Beard]		02/01/2012	Created
      /// </history>
      public static void SetContextMenuItemText(Control holder, MenuItem item)
      {
         SetContextMenuItemText(holder, item, null);
      }

      /// <summary>
      /// Sets the given context menuitem's text property or sub menuitem if specified.
      /// </summary>
      /// <param name="holder">Control containing ContextMenu</param>
      /// <param name="item">MenuItem</param>
      /// <param name="subItem">MenuItem's child MenuItem to set</param>
      /// <history>
      /// [Curtis_Beard]		09/18/2013	ADD: 65, support for contextmenu sub items
      /// </history>
      public static void SetContextMenuItemText(Control holder, MenuItem item, MenuItem subItem)
      {
         if (__RootNode != null)
         {
            string formName = GetParentControl(holder).Name;
            XmlNode node = null;

            if (subItem == null)
            {
               node = __RootNode.SelectSingleNode("screen[@name='" + formName + "']/control[@name='" + holder.Name + "']/menuitem[@index='" + item.Index + "']");
            }
            else
            {
               node = __RootNode.SelectSingleNode("screen[@name='" + formName + "']/control[@name='" + holder.Name + "']/menuitem[@index='" + item.Index + "']/menuitem[@index='" + subItem.Index + "']");
            }

            if (node != null)
            {
               if (node.Attributes["value"] != null)
               {
                  if (subItem == null)
                  {
                     item.Text = node.Attributes["value"].Value;
                  }
                  else
                  {
                     subItem.Text = node.Attributes["value"].Value;
                  }
               }
            }
         }
      }

      /// <summary>
      /// Gets a string value from the generic text section of a language file.
      /// </summary>
      /// <param name="name">Key name to retrieve</param>
      /// <returns>string containing text or string.empty if not found</returns>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static string GetGenericText(string name)
      {
         return GetGenericText(name, string.Empty);
      }

      /// <summary>
      /// Gets a string value from the generic text section of a language file.
      /// </summary>
      /// <param name="name">Key name to retrieve</param>
      /// <param name="defaultValue">Default value to return if not found</param>
      /// <returns>string containing text or given default value if not found</returns>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static string GetGenericText(string name, string defaultValue)
      {
         if (__XmlGenericNode != null)
         {
            XmlNode node = __XmlGenericNode.SelectSingleNode("text[@name='" + name + "']");

            if (node != null && node.Attributes["value"] != null)
               return node.Attributes["value"].Value;
         }

         return defaultValue;
      }

      /// <summary>
      /// Processes the given form for all controls.
      /// </summary>
      /// <param name="frm">Form to process</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static void ProcessForm(Form frm)
      {
         ProcessForm(frm, null);
      }

      /// <summary>
      /// Processes the given form for all controls.
      /// </summary>
      /// <param name="frm">Form to process</param>
      /// <param name="tip">ToolTip control</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static void ProcessForm(Form frm, ToolTip tip)
      {
         if (__RootNode != null)
         {
            SetFormText(frm);

            //process controls on form
            foreach (Control control in frm.Controls)
            {
               if (control.GetType() == typeof(ToolStrip) || control.GetType() == typeof(StatusStrip))
                  ProcessToolStrip((ToolStrip)control, tip);
               else
                  ProcessControl(control, tip);
            }

            //process menu items on form
            if (frm.Menu != null)
            {
               foreach (MenuItem item in frm.Menu.MenuItems)
               {
                  XmlNode menuNode = __RootNode.SelectSingleNode("screen[@name='" + frm.Name + "']/menu[@index='" + item.Index + "']");

                  if (menuNode != null && menuNode.Attributes["value"] != null)
                  {
                     item.Text = menuNode.Attributes["value"].Value;

                     ProcessMainMenuItem(item);
                  }
               }
            }

            //process menu strip items on form
            if (frm.MainMenuStrip != null)
            {
               for (int i = 0; i < frm.MainMenuStrip.Items.Count; i++)
               {
                  ToolStripMenuItem item = frm.MainMenuStrip.Items[i] as ToolStripMenuItem;
                  XmlNode menuNode = __RootNode.SelectSingleNode("screen[@name='" + frm.Name + "']/menu[@index='" + i + "']");

                  if (menuNode != null && menuNode.Attributes["value"] != null)
                  {
                     item.Text = menuNode.Attributes["value"].Value;
                     List<int> indexes = new List<int>();
                     indexes.Add(i);
                     ProcessMainToolStripMenuItem(item, indexes);
                  }
               }
            }
         }
      }

      #region MenuItem Methods

      private static void ProcessMainMenuItem(MenuItem mainMenuItem)
      {
         if (mainMenuItem.MenuItems != null && mainMenuItem.MenuItems.Count > 0)
         {
            foreach (MenuItem item in mainMenuItem.MenuItems)
            {
               ProcessMenuItems(item, mainMenuItem.Index);
            }
         }
      }

      private static void ProcessMenuItems(MenuItem menuItem, int mainMenuIndex)
      {
         if (menuItem.MenuItems.Count == 0)
         {
            SetMenuItemText(menuItem, mainMenuIndex);
         }
         else
         {
            // set text, then process children
            SetMenuItemText(menuItem, mainMenuIndex);

            foreach (MenuItem subItem in menuItem.MenuItems)
            {
               ProcessMenuItems(subItem, mainMenuIndex);
            }
         }
      }

      private static void SetMenuItemText(MenuItem item, int mainMenuIndex)
      {
         if (__RootNode != null)
         {
            string formName = item.GetMainMenu().GetForm().Name;
            MenuItem mainMenuItem = item.GetMainMenu().MenuItems[mainMenuIndex];
            Menu parentItem = item.Parent;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            // start at current level
            builder.Insert(0, string.Format("/menuitem[@index='{0}']", item.Index));

            while (parentItem != null && parentItem is MenuItem && mainMenuItem != (parentItem as MenuItem))
            {
               MenuItem currentItem = parentItem as MenuItem;
               builder.Insert(0, string.Format("/menuitem[@index='{0}']", currentItem.Index));

               parentItem = currentItem.Parent;
            }

            builder.Insert(0, string.Format("screen[@name='{0}']/menu[@index='{1}']", formName, mainMenuIndex));

            XmlNode node = __RootNode.SelectSingleNode(builder.ToString());
            if (node != null)
            {
               if (node.Attributes["value"] != null)
                  item.Text = node.Attributes["value"].Value;
            }
         }
      }

      #endregion

      #region ToolStrip Methods

      private static void ProcessMainToolStripMenuItem(ToolStripMenuItem mainMenuItem, List<int> indexes)
      {
         if (mainMenuItem.DropDownItems != null && mainMenuItem.DropDownItems.Count > 0)
         {
            for (int i = 0; i < mainMenuItem.DropDownItems.Count; i++)
            {
               indexes.Add(i);
               ProcessToolStripMenuItems(mainMenuItem.DropDownItems[i], indexes);
               indexes.RemoveAt(indexes.Count - 1);
            }
         }
      }

      private static void ProcessToolStripMenuItems(ToolStripItem menuItem, List<int> indexes)
      {
         if ((menuItem is ToolStripMenuItem && ((menuItem as ToolStripMenuItem).DropDownItems == null || (menuItem as ToolStripMenuItem).DropDownItems.Count == 0)) || menuItem is ToolStripSeparator)
         {
            SetToolStripMenuItemText(menuItem, indexes);
         }
         else
         {
            // set text, then process children
            SetToolStripMenuItemText(menuItem, indexes);

            if (menuItem is ToolStripMenuItem)
            {
               var item = menuItem as ToolStripMenuItem;
               for (int i = 0; i < item.DropDownItems.Count; i++)
               {
                  indexes.Add(i);
                  SetToolStripMenuItemText(item.DropDownItems[i], indexes);
                  indexes.RemoveAt(indexes.Count - 1);
               }
            }
         }
      }

      private static void SetToolStripMenuItemText(ToolStripItem item, List<int> indexes)
      {
         if (__RootNode != null)
         {
            string formName = string.Empty;
            ToolStrip owner = item.Owner;
            while (owner is ToolStripDropDownMenu)
            {
               owner = (owner as ToolStripDropDownMenu).OwnerItem.Owner;
            }
            formName = owner.FindForm().Name;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            builder.AppendFormat("screen[@name='{0}']", formName);

            for (int i = 0; i < indexes.Count; i++)
            {
               builder.AppendFormat("/menu{0}[@index='{1}']", i == 0 ? string.Empty : "item", indexes[i]);
            }

            XmlNode node = __RootNode.SelectSingleNode(builder.ToString());
            if (node != null)
            {
               if (node.Attributes["value"] != null)
                  item.Text = node.Attributes["value"].Value;
            }
         }
      }

      #endregion

      /// <summary>
      /// Generates an xml document for the given form with all controls.
      /// </summary>
      /// <param name="frm">Form to process</param>
      /// <param name="path">Fully qualified file path</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static void GenerateXml(Form frm, string path)
      {
         XmlDocument xmlDoc = new XmlDocument();
         XmlNode rootNode;
         XmlAttribute attrib;

         try
         {
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));

            rootNode = xmlDoc.CreateElement("screen");
            attrib = xmlDoc.CreateAttribute("name");
            attrib.Value = frm.Name;
            rootNode.Attributes.Append(attrib);
            attrib = xmlDoc.CreateAttribute("value");
            attrib.Value = frm.Text;
            rootNode.Attributes.Append(attrib);

            //process all controls on form
            foreach (Control control in frm.Controls)
               GenerateXmlControls(control, rootNode, xmlDoc);
            
            xmlDoc.AppendChild(rootNode);

            //process menu items on form
            if (frm.Menu != null)
            {
               foreach (MenuItem item in frm.Menu.MenuItems)
               {
                  XmlNode menuNode = xmlDoc.CreateElement("menu");

                  attrib = xmlDoc.CreateAttribute("index");
                  attrib.Value = item.Index.ToString();
                  menuNode.Attributes.Append(attrib);

                  attrib = xmlDoc.CreateAttribute("value");
                  attrib.Value = item.Text;
                  menuNode.Attributes.Append(attrib);

                  GenerateXmlMenuItems(item, menuNode, xmlDoc);

                  rootNode.AppendChild(menuNode);
               }
            }
            xmlDoc.Save(path);
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.ToString());
         }
      }
      #endregion

      #region Private Methods
      /// <summary>
      /// Process a given control to set its text property.
      /// </summary>
      /// <param name="control">Control to process</param>
      /// <param name="tip">ToolTip for control</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// [Curtis_Beard]		02/01/2012	ADD: support for control's contextmenu
      /// [Curtis_Beard]		09/18/2013	ADD: 65, support for contextmenu sub items
      /// </history>
      private static void ProcessControl(Control control, ToolTip tip)
      {
         if (control.Controls.Count == 0)
         {
            SetControlText(control, tip);

            // set context menu if available
            if (control.ContextMenu != null && control.ContextMenu.MenuItems.Count > 0)
            {
               foreach (MenuItem item in control.ContextMenu.MenuItems)
               {
                  SetContextMenuItemText(control, item);

                  // process any sub menu items
                  if (item.MenuItems != null && item.MenuItems.Count > 0)
                  {
                     foreach (MenuItem subItem in item.MenuItems)
                     {
                        SetContextMenuItemText(control, item, subItem);
                     }
                  }
               }
            }
         }
         else
         {
            foreach (Control child in control.Controls)
            {
               ProcessControl(child, tip);
            }

            SetControlText(control, tip);
         }
      }

      /// <summary>
      /// Process each ToolStripItem in the ToolStrip
      /// </summary>
      /// <param name="control">ToolStrip to process</param>
      /// <param name="tip">ToolTip object</param>
      /// <history>
      /// [Curtis_Beard]		09/27/2012	Initial: 1741735, support ToolStripItem
      /// </history>
      private static void ProcessToolStrip(ToolStrip control, ToolTip tip)
      {
         foreach (ToolStripItem child in control.Items)
         {
            ProcessToolStripItem(child, tip);
         }
      }

      /// <summary>
      /// Process each ToolStripItem.
      /// </summary>
      /// <param name="control">ToolStripItem to process</param>
      /// <param name="tip">ToolTip object</param>
      /// <history>
      /// [Curtis_Beard]		09/27/2012	Initial: 1741735, support ToolStripItem
      /// </history>
      private static void ProcessToolStripItem(ToolStripItem control, ToolTip tip)
      {
         SetToolStripItemText(control, tip);
      }

      ///// <summary>
      ///// Processa given MenuItem to set its text property.
      ///// </summary>
      ///// <param name="item">MenuItem to process</param>
      ///// <param name="index">Index of MenuItem's parent</param>
      ///// <history>
      ///// [Curtis_Beard]		07/31/2006	Created
      ///// </history>
      //private static void ProcessMenuItem(MenuItem item, int index)
      //{
      //   if (item.MenuItems.Count == 0)
      //      SetMenuItemText(item, index);
      //   else
      //   {
      //      foreach (MenuItem child in item.MenuItems)
      //         ProcessMenuItem(child, index);

      //      //SetMenuItemText(item, index);
      //   }
      //}

      /// <summary>
      /// Set a given form's text property.
      /// </summary>
      /// <param name="frm">Form to set text</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      public static void SetFormText(Form frm)
      {
         if (__RootNode != null)
         {
            XmlNode node = __RootNode.SelectSingleNode("screen[@name='" + frm.Name + "']");

            if (node != null && node.Attributes["value"] != null )
            {
               //node found
               frm.Text = node.Attributes["value"].Value;
            }
         }
      }

      /// <summary>
      /// Generate xml data for a given control and its children.
      /// </summary>
      /// <param name="control">Control to process</param>
      /// <param name="rootNode">Root xml node</param>
      /// <param name="xmlDoc">Xml Document</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      private static void GenerateXmlControls(Control control, XmlNode rootNode, XmlDocument xmlDoc)
      {
         if (control.Controls.Count == 0)
            GenerateXmlControl(control, rootNode, xmlDoc);
         else
         {
            foreach (Control child in control.Controls)
               GenerateXmlControls(child, rootNode, xmlDoc);
            
            GenerateXmlControl(control, rootNode, xmlDoc);
         }
      }

      /// <summary>
      /// Generate xml data for a given control.
      /// </summary>
      /// <param name="control">Control to process</param>
      /// <param name="rootNode">Root xml node</param>
      /// <param name="xmlDoc">Xml Document</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      private static void GenerateXmlControl(Control control, XmlNode rootNode, XmlDocument xmlDoc)
      {
         if (rootNode != null)
         {
            XmlNode node = xmlDoc.CreateElement("control");
            XmlAttribute attrib = xmlDoc.CreateAttribute("name");

            if (!control.Name.Equals(string.Empty) && !control.Text.Equals(string.Empty))
            {
               //Name
               attrib.Value = control.Name;
               node.Attributes.Append(attrib);

               //Text
               attrib = xmlDoc.CreateAttribute("value");
               attrib.Value = control.Text;
               node.Attributes.Append(attrib);

               //Tooltip

               rootNode.AppendChild(node);
            }
         }
      }

      /// <summary>
      /// Generate xml data for a given MenuItem control and its children.
      /// </summary>
      /// <param name="item">MenuItem to process</param>
      /// <param name="rootNode">Root xml node</param>
      /// <param name="xmlDoc">Xml Document</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      private static void GenerateXmlMenuItems(MenuItem item, XmlNode rootNode, XmlDocument xmlDoc)
      {
         if (item.MenuItems.Count == 0)
            GenerateXmlMenuItem(item, rootNode, xmlDoc);
         else
         {
            foreach (MenuItem child in item.MenuItems)
               GenerateXmlMenuItems(child, rootNode, xmlDoc);
         }
      }

      /// <summary>
      /// Generate xml data for a given MenuItem control.
      /// </summary>
      /// <param name="item">MenuItem to process</param>
      /// <param name="rootNode">Root xml node</param>
      /// <param name="xmlDoc">Xml Document</param>
      /// <history>
      /// [Curtis_Beard]		07/31/2006	Created
      /// </history>
      private static void GenerateXmlMenuItem(MenuItem item, XmlNode rootNode, XmlDocument xmlDoc)
      {
         if (rootNode != null)
         {
            if (!item.Text.Equals(string.Empty))
            {
               XmlNode node = xmlDoc.CreateElement("menuitem");
               XmlAttribute attrib = xmlDoc.CreateAttribute("index");

               //index
               attrib.Value = item.Index.ToString();
               node.Attributes.Append(attrib);

               //Text
               attrib = xmlDoc.CreateAttribute("value");
               attrib.Value = item.Text;
               node.Attributes.Append(attrib);

               rootNode.AppendChild(node);
            }
         }
      }

      /// <summary>
      /// Attempts to load the embedded default language (English)
      /// </summary>
      /// <history>
      /// [Curtis_Beard]		05/22/2007	Created
      /// </history>
      private static void LoadEmbeddedLanguage()
      {
         // load english embedded resource
         System.Reflection.Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
         string assName = thisAssembly.GetName().Name;

         Stream resStream = thisAssembly.GetManifestResourceStream(String.Format("{0}.Language.en-us.xml", assName));

         if (resStream != null)
         {
            string contents = string.Empty;

            using (StreamReader reader = new StreamReader(resStream))
            {
               contents = reader.ReadToEnd();
            }
            resStream.Close();

            if (!contents.Equals(string.Empty))
            {
               __XmlDoc = new XmlDocument();
               __XmlDoc.LoadXml(contents);
               contents = string.Empty;

               __RootNode = __XmlDoc.SelectSingleNode("language");

               if (__RootNode != null)
                  __XmlGenericNode = __RootNode.SelectSingleNode("generic");
            }
         }

         thisAssembly = null;
      }

      /// <summary>
      /// Retrieve the language information from the file specified.
      /// </summary>
      /// <param name="path">Full path to language file</param>
      /// <param name="displayName">Return, language display name</param>
      /// <param name="culture">Return, language culture string</param>
      /// <history>
      /// [Curtis_Beard]		05/22/2007	Created
      /// </history>
      private static void GetLanguageInformation(string path, ref string displayName, ref string culture)
      {
         displayName = string.Empty;
         culture = string.Empty;

         try
         {
            if (File.Exists(path))
            {
               XmlDocument doc = new XmlDocument();

               doc.Load(path);

               XmlNode root = doc.SelectSingleNode("language");

               if (root != null && root.Attributes.Count > 0)
               {
                  if (root.Attributes["displayName"] != null)
                     displayName = root.Attributes["displayName"].Value;

                  if (root.Attributes["culture"] != null)
                     culture = root.Attributes["culture"].Value;
               }

               root = null;
               doc = null;
            }
         }
         catch {}
      }

      /// <summary>
      /// Retrieves the top most control (parent) of the given control.
      /// </summary>
      /// <param name="ctrl">Control to find parent</param>
      /// <returns>Control that is the top most parent of the given control</returns>
      private static Control GetParentControl(Control ctrl)
      {
         if (ctrl.Parent == null)
         {
            return ctrl;
         }
         else
         {
            return GetParentControl(ctrl.Parent);
         }
      }
      #endregion
   }

   /// <summary>
   /// Used to contain a language file.
   /// </summary>
   /// <history>
   /// [Curtis_Beard]		05/22/2007	Created
   /// </history>
   internal class LanguageItem
   {
      private string __displayName = string.Empty;
      private string __culture = string.Empty;

      /// <summary>
      /// Creates a new instance of the LanguageItem class.
      /// </summary>
      /// <param name="displayName">Display name</param>
      /// <param name="culture">Culture string</param>
      /// <history>
      /// [Curtis_Beard]		05/22/2007	Created
      /// </history>
      public LanguageItem(string displayName, string culture)
      {
         __displayName = displayName;
         __culture = culture;
      }

      /// <summary>
      /// Gets/Sets the language's display name.
      /// </summary>
      /// <history>
      /// [Curtis_Beard]		05/22/2007	Created
      /// </history>
      public string DisplayName
      {
         get { return __displayName; }
         set { __displayName=value; }
      }

      /// <summary>
      /// Gets/Sets the language's culture string.
      /// </summary>
      /// <history>
      /// [Curtis_Beard]		05/22/2007	Created
      /// </history>
      public string Culture
      {
         get { return __culture; }
         set { __culture=value; }
      }
   }
}
